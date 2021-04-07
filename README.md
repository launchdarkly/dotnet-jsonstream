# LaunchDarkly Streaming JSON for .NET

[![NuGet](https://img.shields.io/nuget/v/LaunchDarkly.JsonStream.svg?style=flat-square)](https://www.nuget.org/packages/LaunchDarkly.JsonStream/)
[![CircleCI](https://circleci.com/gh/launchdarkly/dotnet-jsonstream.svg?style=shield)](https://circleci.com/gh/launchdarkly/dotnet-jsonstream)
[![Documentation](https://img.shields.io/static/v1?label=GitHub+Pages&message=API+reference&color=00add8)](https://launchdarkly.github.io/dotnet-jsonstream)

## Overview

The `LaunchDarkly.JsonStream` library implements a streaming approach to JSON encoding and decoding designed for efficiency at high volume, assuming a text encoding of UTF8. Unlike reflection-based frameworks, it has no knowledge of structs or other complex types; you must explicitly tell it what values and properties to write or read. It was implemented for the [LaunchDarkly .NET SDK](https://github.com/launchdarkly/dotnet-server-sdk) and [LaunchDarkly Xamarin SDK](http://github.com/launchdarkly/xamarin-client-sdk), but may be useful in other applications.

## Supported .NET versions and platform differences

This version of the SDK is built for the following targets:

* .NET Core 3.1+
* .NET Core 2.1+
* .NET 5.0+
* .NET Framework 4.5.2+
* .NET Framework 4.6.1+
* .NET Standard 2.0 (used on other platforms such as Xamarin)

## `System.Text.Json` support

All builds of `LaunchDarkly.JsonStream` except for .NET Framework 4.5.2 and .NET Standard 2.0 make use of the `System.Text.Json` API, which is built into the standard runtime library for .NET Core 3.x and .NET 5.x and is imported as a NuGet package on other platforms. Any types that use the `LaunchDarkly.JsonStream.JsonStreamConverter` attribute will automatically be recognized by `System.Text.Json`'s reflection-based APIs.

The .NET Framework and .NET Standard 2.0 builds of `LaunchDarkly.JsonStream` use a different portable implementation that is not as fast as `System.Text.Json`, but still highly efficient. `System.Text.Json` is not available for .NET Framework 4.5.2. It is available for .NET Standard 2.0, but the .NET Standard 2.0 target of `LaunchDarkly.JsonStream` is also used in Xamarin, and `System.Text.Json` currently has compatibility problems with Xamarin.

The external API of the library is the same regardless, so portable JSON encoding/decoding logic can be written against `LaunchDarkly.JsonStream` without needing to know the target platform.

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
