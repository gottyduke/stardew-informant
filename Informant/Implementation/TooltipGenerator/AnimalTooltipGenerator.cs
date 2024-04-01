using Microsoft.Xna.Framework;
using Slothsoft.Informant.Api;
using StardewValley.Characters;

namespace Slothsoft.Informant.Implementation.TooltipGenerator;

internal class AnimalTooltipGenerator(IModHelper modHelper) : ITooltipGenerator<Character>
{
    public static bool DecoratePet { get; set; }

    private const int _friendship_max = Pet.maxFriendship;
    private const int _friendship_per_heart = 200;
    private const int _friendship_max_level = _friendship_max / _friendship_per_heart;
    private static readonly Vector2 _friendship_scale = new(3, 3);
    private static readonly Rectangle _friendship_full = new(211, 428, 7, 6);
    private static readonly Rectangle _friendship_left_half = new(211, 428, 4, 6);
    private static readonly Rectangle _friendship_right_half = new(222, 428, 3, 6);
    private static readonly Rectangle _friendship_hollow = new(218, 428, 7, 6);

    public string Id => "animal";
    public string DisplayName => modHelper.Translation.Get("AnimalTooltipGenerator");
    public string Description => modHelper.Translation.Get("AnimalTooltipGenerator.Description");

    public bool HasTooltip(Character input)
    {
        return DecoratePet && input is Pet || input is FarmAnimal;
    }

    public Tooltip Generate(Character input)
    {
        if (input is FarmAnimal animal) {
            return CreateTooltip(modHelper, animal.displayName, animal.friendshipTowardFarmer.Value);
        }

        if (input is Pet pet) {
            return CreateTooltip(modHelper, pet.displayName, pet.friendshipTowardFarmer.Value);
        }

        // something is wrong 
        return CreateTooltip(modHelper, "???", 0);
    }

    internal static Tooltip CreateTooltip(IModHelper modHelper, string name, int friendship)
    {
        var displayName = name;
        var textLength = Game1.smallFont.MeasureString(displayName).X;
        var charLength = Game1.smallFont.MeasureString(" ").X;
        // with a 15 pixels padding in total
        var minimumLength = Math.Max(textLength, _friendship_scale.X * _friendship_full.Width * 5 + 15);
        // extra space for icons
        if (textLength < minimumLength) {
            displayName += new string(' ', (int)((minimumLength - textLength) / charLength) + 1);
        }
        // extra line for icons
        displayName += "\n";

        var love = friendship;
        var hearts = Enumerable
            .Range(1, _friendship_max_level)
            .Select(i => love >= i * _friendship_per_heart
                ? new Rectangle[] { _friendship_full }
                : (love >= (i - 1) * _friendship_per_heart + _friendship_per_heart / 2
                    ? [_friendship_left_half, _friendship_right_half]
                    : [_friendship_hollow]))
            .SelectMany(rects => rects
                .Select(t => new Icon(Game1.mouseCursors) {
                    SourceRectangle = t,
                    Position = IPosition.BottomCenter,
                    IconSize = t.Size.ToVector2() * _friendship_scale,
                }))
            .ToArray();

        return new Tooltip(displayName) {
            Icon = hearts
        };
    }
}