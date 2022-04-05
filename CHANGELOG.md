# Change log

All notable changes to the project will be documented in this file. For full release notes for the projects that depend on this project, see their respective changelogs. This file describes changes only to the common code. This project adheres to [Semantic Versioning](http://semver.org).

## [1.1.0] - 2022-04-05
### Added:
- Added .NET 6.0 as a specific target framework. This should make no practical difference since the .NET 5.0 build was already usable in .NET 6.0, but since 6.0 is a long-term-support version and 5.0 is not, we are adding 6.0 explicitly in all of our libraries.

Note that this library will likely become obsolete in a future SDK release, because its only purpose was to support streaming JSON conversion on platforms that do not support `System.Text.Json`, and the only such platforms remaining (.NET Framework 4.5.2, and the pre-.NET-6.0 Xamarin frameworks) will eventually be unsupported.

## [1.0.3] - 2021-10-18
### Fixed:
- When using the adapter that allows SDK types to be deserialized with the `System.Text.Json` API, temporary `JsonDocument` instances are now disposed of immediately rather than leaving them to be garbage-collected. (Thanks, [JeffAshton](https://github.com/launchdarkly/dotnet-jsonstream/pull/8)!)

## [1.0.2] - 2021-06-07
### Fixed:
- On platforms where `System.Text.Json` is not available, the default JSON parsing implementation was broken for numbers that have an exponent but do not have a decimal (such as `1e-5`, as opposed to `1.0e-5`). For such numbers, the parser was incorrectly throwing a syntax error.

## [1.0.1] - 2021-04-07
### Changed:
- The .NET Standard 2.0 build now uses the same portable custom implementation as .NET Standard 4.5.2, and does not use `System.Text.Json`. This is because the .NET Standard 2.0 build is used by Xamarin projects, and `System.Text.Json` currently does not work in Xamarin.
- To preserve the previous behavior for .NET Core 2.x projects that were using the .NET Standard 2.0 build, a .NET Core 2.1 build has been added which _does_ use `System.Text.Json`.

## [1.0.0] - 2021-02-02
Initial release of this package, for use in the LaunchDarkly .NET SDK 6.0 and Xamarin SDK 2.0.
