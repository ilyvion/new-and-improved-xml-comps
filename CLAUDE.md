# CLAUDE.md — New and Improved XML Comps

A library of comps, mod extensions and stats that let other mods do with XML what would otherwise take C#. Its users are mod authors, so the documentation they read is the GitHub wiki.

## Keeping the wiki up to date

The wiki is a separate git repo, cloned as the sibling directory `../new-and-improved-xml-comps.wiki` (`git@github.com:ilyvion/new-and-improved-xml-comps.wiki.git`; clone it there if it's missing). Each XML-facing feature has one page, named after the class or def mod authors write in XML (e.g. `CompProperties_ImprovedPower.md`, `NewAndImprovedXmlComps_PowerGenFactor.md`).

Whenever a change adds, changes or removes something mod authors use from XML (a comp, comp property field, mod extension, stat, or a change to how one behaves), update the wiki in the same task:

- New feature: add a page in the style of the existing ones (what it does, each field, and an XML example), and add a wiki-linked entry for it to `SteamDescription.txt`'s feature list.
- Changed feature: update its page's description, fields and examples to match.
- Removed feature: delete its page and its `SteamDescription.txt` entry.

Don't commit or push wiki changes; tell the user they're in the wiki repo for review. When producing a commit message, also produce one for the Wiki if it was edited during the session.

@CLAUDE.SHARED.md
