# About these benchmarks

This code uses the BenchmarkDotNet framework to compare performance of a standard set of
JSON encoding and decoding tasks across different implementations:

* The JReader/JWriter API in this library.
* An equivalent usage of the streaming APIs in `System.Text.Json` (which JReader and JWriter
use under the covers, except in .NET Framework 4.5.x).
* An equivalent usage of the streaming APIs in `Newtonsoft.Json`.
* Using the reflection-based APIs in `Newtonsoft.Json` to read and write the same types.

For each implementation, each set of data is encoded and decoded twice: once to/from a
`string`, and once to/from a UTF-8 byte array. This is to make it easier to see how much of
the execution time in the benchmarks is being spent transcoding between strings (which in
.NET are stored as arrays of UTF-16 values) and UTF-8 bytes; `Newtonsoft.Json` operates
directly on the former, whereas the other implementations operate on the latter, so which
implementation is more efficient will depend on which format the application is using.
