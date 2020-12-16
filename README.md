# LaunchDarkly Streaming JSON for .NET

[![Circle CI](https://circleci.com/gh/launchdarkly/dotnet-jsonstream.svg?style=shield)](https://circleci.com/gh/launchdarkly/dotnet-jsonstream)

## This is incomplete prerelease code

This repository is still under preliminary development. It has been made public in order to allow other internal projects to reference it during development and testing.

## Overview

The `LaunchDarkly.JsonStream` library implements a streaming approach to JSON encoding and decoding designed for efficiency at high volume. Unlike reflection-based frameworks, it has no knowledge of structs or other complex types; you must explicitly tell it what values and properties to write or read. It was implemented for the [LaunchDarkly .NET SDK](https://github.com/launchdarkly/dotnet-server-sdk) and [LaunchDarkly Xamarin SDK](http://github.com/launchdarkly/xamarin-client-sdk), but may be useful in other applications.

On platforms that provide the `System.Text.Json` API (.NET Core 3.1+, .NET 5.0+), this works as a wrapper for `System.Text.Json.Utf8JsonReader` and `System.Text.Json.Utf8Jsonwriter`, providing a more convenient API for common JSON parsing and writing operations while taking advantage of the very efficient implementation of these core types. On platforms that do not have `System.Text.Json`, it falls back to a portable implementation that is not as fast as <c>System.Text.Json</c> but still highly efficient. Portable JSON unmarshaling logic can therefore be written against this API without needing to know the target platform.

## Supported .NET versions

This version of the SDK is built for the following targets:

* .NET Core 3.1: runs on .NET Core 3.1+, using `System.Text.Json`.
* .NET 5.0: runs on .NET 5.x, using `System.Text.Json`.
* .NET Framework 4.5.2: runs on .NET Framework 4.5.2 and above. This uses the portable implementation instead of `System.Text.Json`.
* .NET Standard 2.0: runs on .NET Core 2.x, in an application; or within a library that is targeted to .NET Standard 2.x. This uses the portable implementation instead of `System.Text.Json`.

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
    * [Feature Flagging Guide](https://github.com/launchdarkly/featureflags/  "Feature Flagging Guide") for best practices and strategies
