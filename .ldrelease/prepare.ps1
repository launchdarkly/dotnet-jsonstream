
# Extra pre-build step to install a newer .NET SDK than what CircleCI has,
# because we need to be able to build for .NET Core 3.1.

& "./dotnet-install.ps1" -Channel LTS
