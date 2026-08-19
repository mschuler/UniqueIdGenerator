# Security Policy

## Supported Versions

Security fixes are provided for the latest published version of
`UniqueIdGenerator`.

| Version | Supported |
| --- | --- |
| Latest release | ✅ |
| Older releases | ❌ |

## Reporting a Vulnerability

Please report suspected vulnerabilities privately through
[GitHub Security Advisories](https://github.com/mschuler/UniqueIdGenerator/security/advisories/new).

Do not disclose suspected vulnerabilities in public issues or discussions
before a fix is available.

Include, where possible:

- affected package version and target framework
- a minimal reproduction or proof of concept
- expected and actual behaviour
- security impact and any relevant deployment assumptions

Please do not include credentials, personal data, or production data in a
report.

## Scope and Security Boundaries

This repository contains a local .NET library for generating and inspecting
64-bit, time-ordered identifiers. It does not provide authentication,
authorization, encryption, storage, network services, or tenant isolation.

Generated identifiers must not be treated as secrets, bearer tokens, or an
access-control mechanism. Their timestamp, generator ID, and sequence data can
be derived from the identifier.

## Security-Relevant Invariants

Reports are particularly relevant if the library can violate any of these
properties under its documented operating conditions:

- generated IDs collide despite distinct generator IDs, a shared UTC start
  date, and a non-decreasing system clock;
- ID conversion or inspection produces an incorrect value or exposes memory;
- malformed input can cause unexpected execution beyond a normal validation
  failure;
- a documented public API permits an attacker to cause disproportionate
  resource consumption in a realistic application integration.

A `Generator` instance is intentionally not thread-safe. Using the same
generator ID in multiple concurrent processes, sharing a generator instance
between concurrent workers, configuring incompatible start dates, or allowing
the system clock to move backwards can compromise uniqueness or ordering.
Those deployment violations are not, by themselves, security vulnerabilities
in this library.
