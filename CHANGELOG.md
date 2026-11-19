# Changelog

All FileParty packages (`FileParty.Core`, `FileParty.Providers.FileSystem`, `FileParty.Providers.AWS.S3`,
`FileParty.Providers.Azure.Storage`) share one version number.

## 2.1.0

A drop-in upgrade from 1.3.x and 2.0.0: change only the FileParty version. There are no public API changes,
and no dependency upgrades are forced on you.

### Supported frameworks

- `netstandard2.0` (.NET Framework 4.6.2+ including 4.8), `netstandard2.1`, `net8.0`, `net10.0`.

### Dependency ranges (lowered so existing consumers resolve without upgrades)

| Dependency | 2.0.0 | 2.1.0 |
| ---------- | ----- | ----- |
| Microsoft.Extensions.DependencyInjection | >= 9.0.0 (all targets) | >= 5.0.0 (netstandard2.x), >= 8.0.0 (net8.0), >= 9.0.0 (net10.0) |
| AWSSDK.S3 | [3.7.400, 5.0.0) | [3.7.400, 5.0.0) (unchanged) |
| AWSSDK.SecurityToken | [3.7.400, 5.0.0) | [3.7.1, 5.0.0) |
| System.Threading.Tasks.Extensions | >= 4.6.3 (from `4.*`, all targets) | >= 4.5.4, netstandard2.0 only |

Checked against consumers pinned to AWSSDK.S3 3.7.415.26, AWSSDK.Core 3.7.402.51,
AWSSDK.SecurityToken 3.7.1.199–3.7.400.33, Microsoft.Extensions.DependencyInjection 9.0.10–10.0.12,
System.Threading.Tasks.Extensions 4.5.4–4.6.3 and Azure.Storage.Blobs 12.13.0, on net48 and net6.0.

### Security

- NuGet audit (direct and transitive dependencies) is clean for every project and build configuration.
- AWS SDK v4 build/test configuration: the AWSSDK.Core floor is now 4.0.3.3, which fixes
  [GHSA-9cvc-h2w8-phrp](https://github.com/advisories/GHSA-9cvc-h2w8-phrp) (CVE-2026-22611, low; affects 4.0.0–4.0.3.2).
  The shipped package's ranges are unchanged. AWSSDK v3.7 is not affected, and v4 consumers choose
  their own AWSSDK.Core version. Use 4.0.3.3 or later.

### Build and CI

- SDK pinned to .NET 10 via `global.json`.
- Package validation runs on every pack against the latest nuget.org release (2.0.0). CI also validates against 1.3.0.
- New `samples/compat` consumer project (net48 + net6.0) runs in CI and fails on NU1605/NU1608.
- Tests also run on `net48` on Windows CI, which exercises the `netstandard2.0` build.

## 2.0.0

- Added `net10.0`; dropped `net6.0` (net6.0 consumers now use the `netstandard2.1` asset).
- AWS S3 provider works with AWSSDK v3.7 and v4.

## 1.3.0

- Added `net8.0`; dependency updates.
