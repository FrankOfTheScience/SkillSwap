
# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Comprehensive GitHub PR template automation with label-based template application
- Enhanced pre-push git hooks with conventional commits validation, testing, and security checks
- Polly resilience patterns with circuit breaker, timeout, and retry policies
- Controller-based API architecture replacing minimal APIs for better organization
- Comprehensive test suite with 216+ passing tests

### Changed
- Refactored API from minimal APIs to controller-based architecture
- Improved PR workflow automation and validation
- Enhanced development workflow with automated quality checks

### Security
- Added secret detection in pre-push hooks
- Implemented secure configuration management practices

---

## [v0.1.2] - 2025-09-24

### Added
- Enhanced CI workflow with tag fetching and GitHub release creation
- Automated changelog generation and version bumping
- Release automation based on PR labels

### Changed
- Improved CI pipeline reliability and release process

## [v0.1.0] - 2025-09-24

### Added
- Initial project setup and basic functionality
- README documentation with project overview

### Changed
- Enhanced README header formatting

---

## Release Notes

For detailed release notes and migration guides, see the [Releases](https://github.com/FrankOfTheScience/SkillSwap/releases) page.

## Contributing

When contributing to this project:

1. **Use Semantic Versioning**: Choose the appropriate label (`major`, `minor`, `patch`) for your PR
2. **Follow Templates**: The appropriate PR template will be applied automatically based on your label
3. **Complete Sections**: Fill out all template sections thoroughly
4. **Link Issues**: Use "Fixes #123" to auto-close related issues

The automated pipeline will handle version bumping, changelog generation, and releases based on your PR labels and content.
