namespace SessionLogger.Domain.Baits;

public enum BaitType
{
    // Artificial: Brand + Range + Colour + Size
    Lure = 0,

    // Manufactured, non-lure: Brand + Range + Flavour + Size (boilies, pellets, pop-ups, artificial corn...)
    Bait = 1,

    // Worms, maggots, sweetcorn, bread... - just a name, optionally a Size (e.g. hook-bait sizing)
    Natural = 2
}
