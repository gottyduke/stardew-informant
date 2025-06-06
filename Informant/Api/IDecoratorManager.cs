﻿namespace Slothsoft.Informant.Api;

/// <summary>
///     A general manager that allows hooking new providers for a specific type into.
/// </summary>
public interface IDecoratorManager<TInput>
{
    /// <summary>
    ///     Returns the decorators this manager has.
    /// </summary>
    IEnumerable<IDisplayable> Decorators { get; }

    /// <summary>
    ///     Add a decorator that provides information for a specific type.
    /// </summary>
    /// <param name="decorator">the decorator to add.</param>
    void Add(IDecorator<TInput> decorator);

    /// <summary>
    ///     Removes a decorator that provides information for a specific type.
    /// </summary>
    /// <param name="decoratorId">the decorator's ID to remove.</param>
    void Remove(string decoratorId);
}