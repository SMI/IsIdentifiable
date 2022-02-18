# Changelog

# [Unreleased]

- Added `UpdateStrategy.RedactionWord` to customise the substitution value for PII when updating the database
- Moved redaction code to `IsIdentifiable.Redacting` namespace
- Retargetted at dotnet standard 2.1
- Removed dependency on Terminal.Gui from library (still part of the ii CLI)

# [0.0.2] - 2022-02-10

- Made it easier to subclass `IsIdentifiableAbstractRunner` and add custom reports

# [0.0.1] - 2022-02-07

Initial version

[Unreleased]: https://github.com/SMI/IsIdentifiable/compare/v0.0.2..main
[0.0.2]: https://github.com/SMI/IsIdentifiable/releases/tag/v0.0.2
[0.0.1]: https://github.com/SMI/IsIdentifiable/releases/tag/v0.0.1