# AGENTS Instructions

When testing any workflow that triggers a Streamerbot `doAction` client call, simulate the client call by running the corresponding C# action code in this repository with the same arguments sent by the client call.

Assume each Streamerbot action consists only of the C# code in this repository, and that the action code executes immediately once Streamerbot receives the `doAction` request.

## Compatibility policy (required)

- Treat the hosted HTML dock as a fast-moving client and the C# Streamerbot actions as long-lived server code.
- For every change that introduces or renames any `doAction` argument, maintain backward compatibility in C# by accepting both old and new argument names for at least one full release cycle.
- Prefer additive changes over breaking changes: new behavior should default safely when arguments are missing.
- Do not remove legacy global variable names unless all known clients have been migrated.
- When adding features, include a brief note in the PR summary describing how compatibility with legacy dock/client payloads was preserved.
