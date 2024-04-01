using Microsoft.Xna.Framework.Graphics;
using Slothsoft.Informant.Api;

namespace Slothsoft.Informant.Implementation.Decorator;

internal class Decorator<TInput>(string id, Func<string> displayName, Func<string> description, Func<TInput, Texture2D?> decorator) : IDecorator<TInput>
{
    public string Id { get; } = id;
    public string DisplayName => displayName();
    public string Description => description();

    public bool HasDecoration(TInput input)
    {
        return decorator(input) != null;
    }

    public Decoration Decorate(TInput input)
    {
        return new Decoration(decorator(input)!);
    }
}