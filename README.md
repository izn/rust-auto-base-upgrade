# Rust Auto Base Upgrade

Upgrade your base one block at a time using the resources stored in its TC.

## Install

Download `AutoBaseUpgrade.cszip` from [Releases](https://github.com/izn/rust-auto-base-upgrade/releases) and place it in:

```text
carbon/plugins/
```

## Usage

Open your TC and click **Auto-upgrade**, or use:

```text
/baseupgrade
```

Choose the grade you want, then confirm the upgrade. Resources are taken from the
authorized TC as each block is processed. You can use **Stop upgrade**
at any time to cancel it.

## How it works

- Blocks are upgraded one at a time, with a short delay between them.
- Blocks are never downgraded.
- If the TC runs out of resources, the upgrade stops.

Building skins and other appearance changes are not supported.
