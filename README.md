# LaunchDarkly Streaming JSON for .NET

[![NuGet](https://img.shields.io/nuget/v/LaunchDarkly.JsonStream.svg?style=flat-square)](https://www.nuget.org/packages/LaunchDarkly.JsonStream/)
[![CircleCI](https://circleci.com/gh/launchdarkly/dotnet-jsonstream.svg?style=shield)](https://circleci.com/gh/launchdarkly/dotnet-jsonstream)
[![Documentation](https://img.shields.io/static/v1?label=GitHub+Pages&message=API+reference&color=00add8)](https://launchdarkly.github.io/dotnet-jsonstream)

## Overview

The `LaunchDarkly.JsonStream` library implements a streaming approach to JSON encoding and decoding designed for efficiency at high volume, assuming a text encoding of UTF8. Unlike reflection-based frameworks, it has no knowledge of structs or other complex types; you must explicitly tell it what values and properties to write or read. It was implemented for the [LaunchDarkly .NET SDK](https://github.com/launchdarkly/dotnet-server-sdk) and [LaunchDarkly Xamarin SDK](http://github.com/launchdarkly/xamarin-client-sdk), but may be useful in other applications.

On platforms where `System.Text.Json` is available either in the standard runtime library (.NET Core 3.1+, .NET 5.0+) or as a NuGet package (all other platforms except .NET Framework 4.5.2), this works as a wrapper for `System.Text.Json.Utf8JsonReader` and `System.Text.Json.Utf8Jsonwriter`, providing a more convenient API for common JSON parsing and writing operations while taking advantage of the very efficient implementation of these core types. On platforms that do not have `System.Text.Json` (.NET Framework 4.5.2), or where `System.Text.Json` is technically available but problematic (Xamarin-- see below), it falls back to a portable implementation that is not as fast as `System.Text.Json` but still highly efficient. Portable JSON unmarshaling logic can therefore be written against this API without needing to know the target platform.

## Supported .NET versions

This version of the SDK is built for the following targets:

* .NET Core 3.1: runs on .NET Core 3.1+.
* .NET 5.0: runs on .NET 5.x.
* .NET Framework 4.5.2: runs on .NET Framework 4.5.2 and above. This uses the portable implementation instead of `System.Text.Json`.
* .NET Framework 4.6.1: runs on .NET Framework 4.6.1 and above.
* .NET Standard 2.0: runs on .NET Core 2.x, in an application; or within a library that is targeted to .NET Standard 2.x.
* Xamarin Android and iOS (MonoAndroid7.1, Xamarin.iOS1.0): for all versions of Xamarin Android and Xamarin iOS. These builds do not have any Android-specific or iOS-specific dependencies; they are identical to the .NET Standard 2.0 build (which would normally be used in Xamarin if these targets did not exist), except that they use the portable implementation instead of `System.Text.Json`. This is to avoid known Xamarin compatibility problems with `System.Text.Json`, and also to reduce the size of mobile application binaries.

## Signing

The published version of this assembly is both digitally signed by LaunchDarkly and [strong-named](https://docs.microsoft.com/en-us/dotnet/framework/app-domains/strong-named-assemblies). Building the code locally in the default Debug configuration does not sign the assembly and does not require a key file. The public key file is in this repository at `LaunchDarkly.JsonStream.pk` as well as here:

```
Public Key:
0024000004800000940000000602000000240000525341310004000001000100
9d95e00dd6f9ef0bfc51850129b6b6292b99d4c3a2ab0f35cdfd6879eed457bf
aa79a4c0f848c592727cd6bae3a795eda5533a5c54623918303ecabd1c022da6
fe90b8c3e4b61c96595c0b90ff8019872fec9b763dcc5156083a29bad49cf685
f16d1be32d1a13478d59b4c02b4773ad31dceb7828ab8c21ec5b388e1b90c3b0

Public Key Token: baa039a572ce18d0
```

## Contributing

We encourage pull requests and other contributions from the community. Check out our [contributing guidelines](CONTRIBUTING.md) for instructions on how to contribute to this project.

## About LaunchDarkly

* LaunchDarkly is a continuous delivery platform that provides feature flags as a service and allows developers to iterate quickly and safely. We allow you to easily flag your features and manage them from the LaunchDarkly dashboard.  With LaunchDarkly, you can:
    * Roll out a new feature to a subset of your users (like a group of users who opt-in to a beta tester group), gathering feedback and bug reports from real-world use cases.
    * Gradually roll out a feature to an increasing percentage of users, and track the effect that the feature has on key metrics (for instance, how likely is a user to complete a purchase if they have feature A versus feature B?).
    * Turn off a feature that you realize is causing performance problems in production, without needing to re-deploy, or even restart the application with a changed configuration file.
    * Grant access to certain features based on user attributes, like payment plan (eg: users on the ‘gold’ plan get access to more features than users in the ‘silver’ plan). Disable parts of your application to facilitate maintenance, without taking everything offline.
* LaunchDarkly provides feature flag SDKs for a wide variety of languages and technologies. Check out [our documentation](https://docs.launchdarkly.com/docs) for a complete list.
* Explore LaunchDarkly
    * [launchdarkly.com](https://www.launchdarkly.com/ "LaunchDarkly Main Website") for more information
    * [docs.launchdarkly.com](https://docs.launchdarkly.com/  "LaunchDarkly Documentation") for our documentation and SDK reference guides
    * [apidocs.launchdarkly.com](https://apidocs.launchdarkly.com/  "LaunchDarkly API Documentation") for our API documentation
    * [blog.launchdarkly.com](https://blog.launchdarkly.com/  "LaunchDarkly Blog Documentation") for the latest product updates
