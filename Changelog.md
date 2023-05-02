# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.5.1] - 2023-05-02

### Fixed

- Fixed missing Changelog
- Fixed outdated command line help docs

## [1.5.0] - 2023-04-25

### Fixed

- Fixed Standard Deveation getting added twice.

### Added

- Added ignore= command line option for passing in a list of metrics by name to ignore during compariosn.

### Changed

- Updated .Net Target Version to include .Net 6 and .Net 7

## [1.4.0] - 2023-04-13

### Added

- Added support marking a sample group as having "A Known Issue". Logging these in a seperate results category and not returning 1.

## [1.3.0] - 2023-04-13

### Fixed

- Fixed missing data members in V2 Data Format

### Added

- Added support for parsing json formatted perf data
- Added fileformat= command line option for setting the file format
- Added dataversion= command line option for setting the version V1 or V2 orf the files to be processed for Json Format

## [1.2.0] - 2021-05-17

### Changed

- Updated .Net Target to .Net Core 3.1
- Updated chart.js to V2.9.4

## [1.0.1] - 2020-05-29

### Fixed

- Fix for date format from epoch unix s to ms

## [1.0.0] - 2020-05-08

### Fixed

- Fixed "Show Failed Tests Only" not being togglable in report.

### Added

- Adds support for V2 Perf Data Format

### Changed

- Updated .Net Target to .Net Core 3.0

This is the first release of _Unity Performance Benchmark Reporter_.
