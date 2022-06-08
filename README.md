# BankDumper

This started as a fork of a simple program to dump bank files, now i am trying to make it behave more like a library that anyone could use for simple file analysis.

Any help / suggestions are appreciated.

## Usage example

```cs
using var input = File.Open(args[0], FileMode.Open, FileAccess.ReadWrite, FileShare.Read);

var result = FileTools.Analyze(input);

Console.WriteLine(result.ToString());

foreach (var pattern in result.Matches)
{
    // Do what you want here
}
```
