# lambdabench
Benchmark for differences between direct method call, call through Action&lt;T> and Action&lt;T> created from a lambda expressions

I was trying to figure out the fastest way to invoke a generic method, without knowing in advance what the type argument is.

# Experiments on MethodCallBenchs

This is done to isolate the diferences between calling a static method via an Action and directly.

There is a `DoThingsWithMock` method which takes an instance of a `Mock`class.

- DirectCall is a straight static method call
- CallThroughInstanceMethod calls through an instance method of the current class.
- CallThroughVirtualInstanceMethod calls through a virtual instance method
- WithAction calls the method via an `Action<Mock>` instance
- WithActionFromDirectExpression uses an action created from an expression built directly on the source code
- WithActionFromExpression uses an action created from an expression built manually
- Invoke is the "slow version" reflecting on a method and calling `Invoke` on the `MethodInfo`

## Results

There seems to be four "tiers" of performance:

- Direct calls (calling the method, directly or via instance non-virtual method) (1x, baseline)
- Calling through an indirection (either virtual or an action) (~20x slower)
- Calling through an action created from an expression (200x slower)
- Using Invoke (1000x slower)

Next step would be to try and figure out why calling an action built from an expression is 10x slower than calling though a regular action.

## BenchmarkDotNet Results

Source for benchs is up at https://github.com/brunomlopes/lambdabench/blob/master/LambdaBench/MethodCallBenchs.cs#L41

``` ini

BenchmarkDotNet=v0.10.11, OS=Windows 10 Redstone 2 [1703, Creators Update] (10.0.15063.726)
Processor=Intel Core i7-4770 CPU 3.40GHz (Haswell), ProcessorCount=8
Frequency=3318388 Hz, Resolution=301.3511 ns, Timer=TSC
  [Host]     : .NET Framework 4.6.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2115.0
  DefaultJob : .NET Framework 4.6.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2115.0


```
|                           Method |        Mean |     Error |    StdDev |      Median |
|--------------------------------- |------------:|----------:|----------:|------------:|
|                       DirectCall |   0.0611 ns | 0.0255 ns | 0.0753 ns |   0.0183 ns |
|        CallThroughInstanceMethod |   0.0052 ns | 0.0114 ns | 0.0168 ns |   0.0000 ns |
| CallThroughVirtualInstanceMethod |   1.1833 ns | 0.0428 ns | 0.0379 ns |   1.1828 ns |
|                       WithAction |   1.5987 ns | 0.0436 ns | 0.0408 ns |   1.6023 ns |
|   WithActionFromDirectExpression |  10.8877 ns | 0.2495 ns | 0.2774 ns |  10.8555 ns |
|         WithActionFromExpression |   9.2865 ns | 0.2200 ns | 0.3967 ns |   9.2376 ns |
|                           Invoke | 284.4507 ns | 5.7215 ns | 7.4396 ns | 284.3009 ns |
