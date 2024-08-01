using System.Reflection;
using ilyvion.Laboratory;

namespace NewAndImprovedXmlComps;

public class NewAndImprovedXmlCompsMod : IlyvionMod
{
#pragma warning disable CS8618 // Set by constructor
    internal static NewAndImprovedXmlCompsMod Instance;
#pragma warning restore CS8618

    public NewAndImprovedXmlCompsMod(ModContentPack content) : base(content)
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
