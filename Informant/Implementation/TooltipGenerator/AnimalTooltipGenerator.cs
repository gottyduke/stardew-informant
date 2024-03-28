using Microsoft.Xna.Framework;
using Slothsoft.Informant.Api;

namespace Slothsoft.Informant.Implementation.TooltipGenerator;

internal class AnimalTooltipGenerator : ITooltipGenerator<FarmAnimal>
{

    private readonly IModHelper _modHelper;
    private static Vector2 _friendship_scale = new(3, 3);
    private static Rectangle _friendship_full = new(211, 428, 7, 6);
    private static Rectangle _friendship_left_half = new(211, 428, 4, 6);
    private static Rectangle _friendship_right_half = new(222, 428, 3, 6);
    private static Rectangle _friendship_hollow = new(218, 428, 7, 6);

    public AnimalTooltipGenerator(IModHelper modHelper)
    {
        _modHelper = modHelper;
    }

    public string Id => "animal";
    public string DisplayName => _modHelper.Translation.Get("AnimalTooltipGenerator");
    public string Description => _modHelper.Translation.Get("AnimalTooltipGenerator.Description");

    public bool HasTooltip(FarmAnimal input)
    {
        return true; // always display for now, until more features added
    }

    public Tooltip Generate(FarmAnimal input)
    {
        return CreateTooltip(_modHelper, input);
    }

    internal static Tooltip CreateTooltip(IModHelper modHelper, FarmAnimal animal)
    {
        var displayName = animal.displayName;
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

        var love = animal.friendshipTowardFarmer.Value;
        var hearts = Enumerable
            .Range(1, 5)
            .Select(i => love >= i * 200
                ? new Rectangle[] { _friendship_full }
                : (love >= (i - 1) * 200 + 100
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