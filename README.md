# IsIdentifiable
A tool for detecting identifiable information in data sources.  Out of the box supports:

- CSV
- DICOM
- Relational Database Tables (Sql Server, MySql, Postgres, Oracle)

![Demo Video](/isidentifiable.gif)

Rules base is driven by regular expressions and plugin services (e.g. Natural Language Processing).  Also includes a reviewer/redactor tool for processing false positives and updating the rules base.

- [Detector Documentation](./IsIdentifiable/README.md)
- [Reviewer Documentation](./Reviewer/README.md)
