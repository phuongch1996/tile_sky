# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.3.3] - 2025-09-16
### Fixed
- Signing package
- Fixed assets being loaded despite being of the incorrect type

## [2.3.2] - 2025-08-12

### Fixed
- Compilation when incompatible version of Deployment package is installed

## [2.3.1] - 2025-07-24

### Added
- Added support for 504 error codes.

## [2.3.0] - 2025-05-07

### Fixed
- Included Metadata in Leaderboard Versioned Entry models
- Internal representation of bucket size switched to integer from decimal

### Added
- Added a custom inspector for the configurations asset (`.lb` files)
  - Unity 2022 and higher
- Reset leaderboard command available from Deployment context menu and Leaderboard Inspector

## [2.2.1] - 2024-11-26

### Fixed
- Fixed inspector loading for service assets, below Unity 6
- Fixed button to open in dashboard

## [2.2.0] - 2024-11-12

### Changed
- Updated Apple Privacy Manifest to include the use of gameplay data.

### Added
- View in Deployment Window button for `.lb` file, dependent on Deployment package version 1.4.0.

### Fixed
- Help URLs from Leaderboard assets to the relevant documentation

## [2.1.0] - 2024-06-10

### Added

- Adding service registration to the core services registry
- Adding service access through the core services registry  (`UnityServices.Instance.GetLeaderboardsService()`)
- Adding service support for creating leaderboards service instances through `UnityServices.CreateInstance`

### Fixed
- Moved create leaderboard configuration menu item under "Services"

## [2.0.2] - 2024-04-03

### Changed

*  Updated Apple Privacy Manifest

## [2.0.1] - 2024-02-13

### Added

* Added Apple Privacy Manifest

### Fixed

* Fixed server error handling in editor deployment window

## [2.0.0] - 2023-10-06

* Optional object `metadata` in all score-fetching methods. If populated, the object will be stored alongside the score.
* Optional boolean `includeMetadata` in all score-fetching methods. If true, stored metadata for scores will be returned as part of the `LeaderboardEntry`.
* Optional integer `limit` on `GetVersionsAsync` method. If set, only the most recent `limit` number of archived versions will be returned. 
* `GetVersionsAsync` now returns string `versionId` (the current version of the leaderboard) and int `totalArchivedVersions` (the total number of archived leaderboard versions stored).
* Optional string `versionId` on the `AddPlayerScoreAsync` method. If set, the versionId will be compared to the current live leaderboard versionId. If they do not match, the score will not be submitted.
* Editor support for Config-as-Code.

## [1.0.0] - 2023-02-20

Major release of the Leaderboards SDK, containing some added documentation and changes to the names, parameters, and namespace of the public interface.

### Changed

* Added XML documentation to service methods.
* Updated the signature of service methods:
  * Renamed service methods to omit repetitive "Leaderboards".
  * Wrapped optional parameters in options objects instead of specifying them directly.
* Service methods are now directly on `LeaderboardsService.Instance`, instead of `LeaderboardsService.Instance.LeaderboardsApi`.

## [0.3.0-preview] - 2023-02-17

Incremental release of the Leaderboards SDK, enabling the return of a player entry on score submission, new support for Tiers, and additional archived leaderboard functionality.

### New Features

* `AddLeaderboardPlayerScoreAsync` updated to return the `LeaderboardEntry` stored for the player, if it is returned by the service.
  A service update to enable this functionality will shortly follow this release.
* `GetLeaderboardScoresByTierAsync` and `GetLeaderboardVersionScoresByTierAsync` added, allowing players to retrieve only the subset 
  of the leaderboard specified by the given tier, for either live or archived leaderboard versions. The rank returned by this method
  will be scoped to the tier requested.
* `GetLeaderboardVersionPlayerRangeAsync` and `GetLeaderboardVersionScoresByPlayerIdsAsync`, extending existing live leaderboard
  functionality to archived leaderboard versions.

## [0.2.1-preview] - 2023-01-26

Incremental release of the Leaderboards SDK, containing new return values and improved error handling

### New Features

* The `GetLeaderboardVersionsAsync` response now includes a `nextReset` field, which shows the next time that
  a leaderboard will reset if the leaderboard has a scheduled reset configuration.
* When a request is made to retrieve leaderboard scores from a bucketed leaderboard, and the player has not yet
  submitted a score (and therefore has not been assigned a bucket), the error response will now include 
  `ScoreSubmissionRequired` in the `Reason` field.

## [0.2.0-preview] - 2022-11-02

Incremental release of the Leaderboards SDK, containing new features and routing to updated API endpoints.

### New Features

* Ability to retrieve scores based on a list of player IDs, enabling features such as Friends leaderboards.
* Ability to retrieve a range of scores around the signed in player, for easier player-scoped leaderboards.
* All methods updated to call the v1beta1 endpoints on the Leaderboards service rather than the Alpha v0 endpoints.

### Known Issues

* There is no client-side validation other than type protection.

## [0.1.0-preview] - 2022-10-07

This is the initial release of the Leaderboards SDK with improvements to the auto-generated code.

### New Features

* Auto-injection of ProjectID and PlayerID matching other SDKs.
* Submit and retrieve player scores/ranks.
* Retrieve Leaderboard scores.
* Retrieve a list of Leaderboard versions and scores from Leaderboard versions.

### Known Issues

* There is no client-side validation other than type protection.

## [0.0.1-preview] - 2022-06-01

- Generated this version of the API client
