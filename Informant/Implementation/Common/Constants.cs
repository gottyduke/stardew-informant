// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace Slothsoft.Informant.Implementation.Common;

/// <summary>
/// These constants can be used to compare to <code>Object.ParentSheetIndex</code>.
/// Updated for 1.6
/// </summary>
public static class BigCraftableIds
{
    public const string BeeHouse = "10";
    public const string Cask = "163";
    public const string CheesePress = "16";
    public const string Keg = "12";
    public const string Loom = "17";
    public const string MayonnaiseMachine = "24";
    public const string OilMaker = "19";
    public const string PreservesJar = "15";
    public const string BoneMill = "90";
    public const string CharcoalKiln = "114";
    public const string Crystalarium = "21";
    public const string Furnace = "13";
    public const string GeodeCrusher = "182";
    public const string HeavyTapper = "264";
    public const string LightningRod = "9";
    public const string MushroomBox = "128";
    public const string OstrichIncubator = "254";
    public const string RecyclingMachine = "20";
    public const string SeedMaker = "25";
    public const string SlimeEggPress = "158";
    public const string SlimeIncubator = "156";
    public const string SlimeIncubator2 = "157"; // game has both of these IDs?
    public const string SolarPanel = "231";
    public const string Tapper = "105";
    public const string WoodChipper = "211";
    public const string WormBin = "154";
    public const string Incubator = "101";
    public const string Incubator2 = "102"; // the Wiki above shows Incubator as 101, but the game as 102?
    public const string Incubator3 = "103"; // maybe it's the egg color?
    public const string CoffeeMaker = "246";
    public const string Deconstructor = "265";
    public const string StatueOfPerfection = "160";
    public const string StatueOfTruePerfection = "280";
    // 1.6
    public const string MushroomLog = "MushroomLog";
    public const string BaitMaker = "BaitMaker";
    public const string Dehydrator = "Dehydrator";
    public const string HeavyFurnace = "HeavyFurnace";
    public const string StatueOfBlessings = "StatueOfBlessings";
    public const string StatueOfTheDwarfKing = "StatueOfTheDwarfKing";
    public const string FishSmoker = "FishSmoker";
    public const string DeluxeWormbin = "DeluxeWormBin";

    public static readonly string[] AllMachines = [
        BeeHouse, Cask, CheesePress, Keg, Loom, MayonnaiseMachine, OilMaker, PreservesJar,
        BoneMill, CharcoalKiln, Crystalarium, Furnace, GeodeCrusher, HeavyTapper, LightningRod, MushroomBox, OstrichIncubator, RecyclingMachine, SeedMaker, SlimeEggPress,
        SlimeIncubator, SlimeIncubator2, SolarPanel, Tapper, WoodChipper, WormBin, Incubator, Incubator2, Incubator3, CoffeeMaker, Deconstructor,
        StatueOfPerfection, StatueOfTruePerfection, MushroomLog, BaitMaker, Dehydrator, HeavyFurnace, StatueOfBlessings, StatueOfTheDwarfKing, FishSmoker, DeluxeWormbin,
    ];

    public const string Chest = "130";
    public const string JunimoChest = "256";
    public const string MiniFridge = "216";
    public const string StoneChest = "232";
    public const string MiniShippingBin = "248";
    // 1.6
    public const string BigChest = "BigChest";
    public const string BigStoneChest = "BigStoneChest";

    public static readonly string[] AllChests = [
        Chest, JunimoChest, MiniFridge, StoneChest, MiniShippingBin, BigChest, BigStoneChest,
    ];

    public static readonly string[] HousePlants = Enumerable.Range(0, 7 + 1).Select(id => id.ToString()).ToArray();
    public const string Scarecrow = "8";
    public const string TablePieceL = "22";
    public const string TablePieceR = "23";
    public const string WoodChair = "26";
    public const string WoodChair2 = "27";
    public const string SkeletonModel = "28";
    public const string Obelisk = "29";
    public const string ChickenStatue = "31";
    public const string StoneCairn = "32";
    public const string SuitOfArmor = "33";
    public const string SignOfTheVessel = "34";
    public const string BasicLog = "35";
    public const string LawnFlamingo = "36";
    public const string WoodSign = "37";
    public const string StoneSign = "38";
    public const string DarkSign = "39";
    public const string BigGreenCane = "40";
    public const string GreenCanes = "41";
    public const string MixedCane = "42";
    public const string RedCanes = "43";
    public const string BigRedCane = "44";
    public const string OrnamentalHayBale = "45";
    public const string LogSection = "46";
    public const string GraveStone = "47";
    public const string SeasonalDecor = "48";
    public const string StoneFrog = "52";
    public const string StoneParrot = "53";
    public const string StoneOwl = "54";
    public const string StoneJunimo = "55";
    public const string SlimeBall = "56";
    public const string GardenPot = "62";
    public const string Bookcase = "64";
    public const string FancyTable = "65";
    public const string AncientTable = "66";
    public const string AncientStool = "67";
    public const string GrandfatherClock = "68";
    public const string TeddyTimer = "69";
    public const string DeadTree = "70";
    public const string Staircase = "71";
    public const string TallTorch = "72";
    public const string RitualMask = "73";
    public const string Bonfire = "74";
    public const string Bongo = "75";
    public const string DecorativeSpears = "76";
    public const string Boulder = "78";
    public const string Door = "79";
    public const string Door2 = "80";
    public const string LockedDoor = "81";
    public const string LockedDoor2 = "82";
    public const string WickedStatue = "83";
    public const string WickedStatue2 = "84";
    public const string SlothSkeletonL = "85";
    public const string SlothSkeletonM = "86";
    public const string SlothSkeletonR = "87";
    public const string StandingGeode = "88";
    public const string ObsidianVase = "89";
    public const string SingingStone = "94";
    public const string StoneOwl2 = "95";
    public const string StrangeCapsule = "96";
    public const string EmptyCapsule = "98";
    public const string FeedHopper = "99";
    public const string Heater = "104";
    public const string Camera = "106";
    public const string PlushBunny = "107";
    public const string TubOFlowers = "108";
    public const string TubOFlowers2 = "109";
    public const string Rarecrow = "110";
    public const string DecorativePitcher = "111";
    public const string DriedSunflowers = "112";
    public const string Rarecrow2 = "113";
    public const string StardewHeroTrophy = "116";
    public const string SodaMachine = "117";
    public static readonly string[] BarrelsAndCrates = Enumerable.Range(118, 125 - 118 + 1).Select(id => id.ToString()).ToArray();
    public const string Rarecrow3 = "126";
    public const string StatueOfEndlessFortune = "127";
    public const string Rarecrow4 = "136";
    public const string Rarecrow5 = "137";
    public const string Rarecrow6 = "138";
    public const string Rarecrow7 = "139";
    public const string Rarecrow8 = "140";
    public const string PrairieKingArcadeSystem = "141";
    public const string WoodenBrazier = "143";
    public const string StoneBrazier = "144";
    public const string GoldBrazier = "145";
    public const string Campfire = "146";
    public const string StumpBrazier = "147";
    public const string CarvedBrazier = "148";
    public const string SkullBrazier = "149";
    public const string BarrelBrazier = "150";
    public const string MarbleBrazier = "151";
    public const string WoodLamppost = "152";
    public const string IronLamppost = "153";
    public const string Hmtgf = "155";
    public const string JunimoKartArcadeSystem = "159";
    public const string PinkyLemon = "161";
    public const string Foroguemo = "162";
    public const string SolidGoldLewis = "164";
    public const string AutoGrabber = "165";
    public const string DeluxeScarecrow = "167";
    public const string Barrel = "174";
    public const string Crate = "175";
    public static readonly string[] SeasonalPlants = Enumerable.Range(184, 207 - 184 + 1).Select(id => id.ToString()).ToArray();
    public const string Workbench = "208";
    public const string MiniJukebox = "209";
    public const string Telephone = "214";
    public const string CursedPkArcadeSystem = "219";
    public const string MiniObelisk = "238";
    public const string FarmComputer = "239";
    public const string SewingMachine = "247";
    public const string AutoPetter = "272";
    public const string Hopper = "275";
    public const string Campfire2 = "278";
    // 1.6
    public const string TextSign = "TextSign";
    public const string Anvil = "Anvil";

    public static readonly string[] AllStaticCraftables =
    [
        .. HousePlants,
        .. BarrelsAndCrates,
        .. SeasonalPlants,
        .. new[] {
            Scarecrow, TablePieceL, TablePieceR, WoodChair, WoodChair2, SkeletonModel, Obelisk, ChickenStatue, StoneCairn, SuitOfArmor, SignOfTheVessel, BasicLog,
            LawnFlamingo, WoodSign, StoneSign, DarkSign, BigGreenCane, GreenCanes, MixedCane, RedCanes, BigRedCane, OrnamentalHayBale, LogSection, GraveStone,
            SeasonalDecor, StoneFrog, StoneParrot, StoneOwl, StoneJunimo, SlimeBall, GardenPot, Bookcase, FancyTable, AncientTable, AncientStool, GrandfatherClock,
            TeddyTimer, DeadTree, Staircase, TallTorch, RitualMask, Bonfire, Bongo, DecorativeSpears, Boulder, Door, Door2, LockedDoor, LockedDoor2, WickedStatue,
            WickedStatue2, SlothSkeletonL, SlothSkeletonM, SlothSkeletonR, StandingGeode, ObsidianVase, SingingStone, StoneOwl2, StrangeCapsule, EmptyCapsule,
            FeedHopper, Heater, Camera, PlushBunny, TubOFlowers, TubOFlowers2, Rarecrow, DecorativePitcher, DriedSunflowers, Rarecrow2, StardewHeroTrophy,
            SodaMachine, Rarecrow3, StatueOfEndlessFortune, Rarecrow4, Rarecrow5, Rarecrow6, Rarecrow7, Rarecrow8,
            PrairieKingArcadeSystem, WoodenBrazier, StoneBrazier, GoldBrazier, Campfire, StumpBrazier, CarvedBrazier, SkullBrazier, BarrelBrazier, MarbleBrazier,
            WoodLamppost, IronLamppost, Hmtgf, JunimoKartArcadeSystem, PinkyLemon, Foroguemo, SolidGoldLewis, AutoGrabber, DeluxeScarecrow, Barrel, Crate,
            Workbench, MiniJukebox, Telephone, CursedPkArcadeSystem, MiniObelisk, FarmComputer, SewingMachine, AutoPetter, Hopper, Campfire2, TextSign, Anvil,
        },
    ];

    public static readonly string[] AllRarecrows = [
        Rarecrow, Rarecrow2, Rarecrow3, Rarecrow4, Rarecrow5, Rarecrow6, Rarecrow7, Rarecrow8,
    ];
}

public static class CropIds
{
    // Ship 15 of each crop
    public static readonly string[] Polyculture = new[] {
        24, 188, 190, 192, 248, 250, 252, 254, 256, 258, 260, 262, 264, 266, 268, 270, 272, 274, 276, 278, 280, 282, 284, 300, 304, 398, 400, 433
    }.Select(id => id.ToString()).ToArray();

    public const string Ginger = "829";
    public const string MixedFlowers = "MixedFlowerSeeds";
    public const string MixedSeeds = "770";
    public const string GreenTeaBush = "251";
    public const string GreenTeaLeaves = "815";
    public const string MossySeed = "MossySeed";
    public const string Moss = "Moss";
}

// for new SVE wild trees
public static class SveTreeIds
{
    public const string BirchTree = "FlashShifter.StardewValleyExpandedCP_Birch_Tree";
    public const string FirTree = "FlashShifter.StardewValleyExpandedCP_Fir_Tree";
}