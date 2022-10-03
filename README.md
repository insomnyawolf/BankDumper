# BankDumper

This started as a fork of a simple program to dump bank files, now i am trying to make it behave more like a library that anyone could use for simple file analysis.

Any help / suggestions are appreciated.

## Usage example

```cs
var patternSearch = new PatternSearch();

patternSearch.AddPatternAKPK();
patternSearch.AddPatternBKHD();
patternSearch.AddPatternFSB5();

using var input = File.Open(args[0], FileMode.Open, FileAccess.Read, FileShare.Read);

patternSearch.SetStream(input);

var result = patternSearch.Analyze();

foreach (var pattern in result)
{
    // Do what you want here
}
```
