#!/bin/sh

# Run this script to use a local copy of g0v0 rather than fetching it from nuget.
# It expects the g0v0 directory to be at the same level as the osu-performance-server directory


CSPROJ="PerformanceServer/PerformanceServer.csproj"
SLN="osu-performance-server.sln"

DEPENDENCIES="../g0v0/osu.Game.Rulesets.Catch/osu.Game.Rulesets.Catch.csproj
    ../g0v0/osu.Game.Rulesets.Mania/osu.Game.Rulesets.Mania.csproj
    ../g0v0/osu.Game.Rulesets.Osu/osu.Game.Rulesets.Osu.csproj
    ../g0v0/osu.Game.Rulesets.Taiko/osu.Game.Rulesets.Taiko.csproj
    ../g0v0/osu.Game/osu.Game.csproj"


dotnet remove $CSPROJ package g0v0.osu.Game
dotnet remove $CSPROJ package g0v0.osu.Game.Rulesets.Osu
dotnet remove $CSPROJ package g0v0.osu.Game.Rulesets.Taiko
dotnet remove $CSPROJ package g0v0.osu.Game.Rulesets.Catch
dotnet remove $CSPROJ package g0v0.osu.Game.Rulesets.Mania

dotnet sln $SLN add $DEPENDENCIES
dotnet add $CSPROJ reference $DEPENDENCIES