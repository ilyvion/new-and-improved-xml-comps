namespace NewAndImprovedXmlComps;

/// <summary>
/// The main mod class for New and Improved XML Comps.
/// </summary>
public class NewAndImprovedXmlCompsMod : IlyvionMod
{
#pragma warning disable CS8618 // Set by constructor
    internal static NewAndImprovedXmlCompsMod Instance;
#pragma warning restore CS8618

    /// <summary>
    /// Initializes a new instance of the <see cref="NewAndImprovedXmlCompsMod"/> class.
    /// </summary>
    /// <param name="content">The mod content pack.</param>
    public NewAndImprovedXmlCompsMod(ModContentPack content)
        : base(content)
    {
        // This is kind of stupid, but also kind of correct. Correct wins.
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        Instance = this;

        // apply fixes
        var harmony = new Harmony(content.Name);
        //Harmony.DEBUG = true;
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        //Harmony.DEBUG = false;
    }
}
