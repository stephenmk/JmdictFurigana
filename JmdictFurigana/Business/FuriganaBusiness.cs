using JmdictFurigana.Models;
using JmdictFurigana.Business.Solvers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace JmdictFurigana.Business;

/// <summary>
/// Works with kanji and dictionary entries to attach each entry a furigana string.
/// </summary>
public class FuriganaBusiness
{
    #region Properties

    /// <summary>
    /// Gets or sets the resource set that will be sent to solvers.
    /// </summary>
    public FuriganaResourceSet ResourceSet { get; set; }

    /// <summary>
    /// Gets or sets the furigana solver list.
    /// </summary>
    public List<FuriganaSolver> Solvers { get; set; }

    /// <summary>
    /// Gets or sets the dictionary file to use.
    /// </summary>
    public DictionaryFile DictionaryFile { get; set; }

    #endregion

    #region Constructors

    public FuriganaBusiness(DictionaryFile dictionaryFile)
    {
        DictionaryFile = dictionaryFile;
        Initialize();
    }

    public FuriganaBusiness(DictionaryFile dictionaryFile, FuriganaResourceSet resourceSet)
        : this(dictionaryFile)
    {
        ResourceSet = resourceSet;
    }

    public FuriganaBusiness(DictionaryFile dictionaryFile, FuriganaResourceSet resourceSet, List<FuriganaSolver> solvers)
        : this(dictionaryFile, resourceSet)
    {
        Solvers = solvers;
    }

    #endregion

    #region Methods

    protected void Initialize()
    {
        if (ResourceSet == null)
        {
            ResourceSet = new FuriganaResourceSet();
            ResourceSet.Load();
        }

        Solvers ??= [
            new KanaReadingSolver(),
            new KanjiReadingSolver(useNanori: DictionaryFile == DictionaryFile.Jmnedict),
            new LengthMatchSolver(),
            new NoConsecutiveKanjiSolver(),
            new OverrideSolver(),
            new RepeatedKanjiSolver(),
            new SingleCharacterSolver(),
            new SingleKanjiSolver(),
        ];

        Solvers.Sort();
        Solvers.Reverse();
    }

    /// <summary>
    /// Starts the process of associating a furigana string to vocab.
    /// </summary>
    /// <returns>The furigana vocab entries.</returns>
    public async IAsyncEnumerable<FuriganaSolutionSet> ExecuteAsync(IAsyncEnumerable<VocabEntry> vocab)
    {
        var processingTasks = new List<Task<FuriganaSolutionSet>>();
        await foreach (var v in vocab)
        {
            var task = Task.Run(() => Execute(v));
            processingTasks.Add(task);
        }
        await foreach (var task in Task.WhenEach(processingTasks))
        {
            yield return await task;
        }
    }

    public FuriganaSolutionSet Execute(VocabEntry v)
    {
        if (string.IsNullOrWhiteSpace(v.KanjiReading) || string.IsNullOrWhiteSpace(v.KanaReading))
        {
            // Cannot solve when we do not have a kanji or kana reading.
            return new FuriganaSolutionSet(v);
        }

        var result = Process(v);
        if (!result.Any() && v.KanjiReading.StartsWith('御'))
        {
            // When a word starts with 御 (honorific, often used), try to override the
            // result by replacing it with an お or a ご. It will sometimes bring a
            // result where the kanji form wouldn't.

            result = Process(new VocabEntry(v.KanaReading, "お" + v.KanjiReading[1..]));

            if (!result.Any())
            {
                result = Process(new VocabEntry(v.KanaReading, "ご" + v.KanjiReading[1..]));
            }

            result.Vocab = v;
        }

        return result;
    }

    private FuriganaSolutionSet Process(VocabEntry v)
    {
        var solutionSet = new FuriganaSolutionSet(v);

        int priority = Solvers.First().Priority;
        foreach (var solver in Solvers)
        {
            if (solver.Priority < priority)
            {
                if (solutionSet.Any())
                {
                    // Priority goes down and we already have solutions.
                    // Stop solving.
                    break;
                }

                // No solutions yet. Continue with the next level of priority.
                priority = solver.Priority;
            }

            // Add all solutions if they are correct and unique.
            solutionSet.SafeAdd(solver.Solve(ResourceSet, v));
        }

        return solutionSet;
    }

    #endregion
}
