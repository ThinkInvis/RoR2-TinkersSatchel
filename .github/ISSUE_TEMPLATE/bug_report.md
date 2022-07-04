---
name: Bug report
about: Create a report to help us improve
title: ''
labels: ''
assignees: ''

---

## IMPORTANT: Read Me First!
To maximize developers' ability to resolve your issue, please fill out this template as completely as possible. Specifically: **DO NOT OMIT the Console Log section**, as chances of being able to resolve a bug report are minimal without a console log. This readme section, and the template text in other sections, need not be included in the finalized report.

See below some other common problems with bug reports. **Your issue may be closed without support if any of these are true**:

- *The bug has only been produced in a very large list of active mods.* Larger modlists (~50 or more) become increasingly difficult to debug, as any two or more of the mods might conflict.
	- Please try to provide a Minimum Working Example of the bug: the smallest modlist and set of any other variables necessary for the bug to occur.
- *The bug has only been produced in the presence of outdated mods.* Any mods that haven't had updates released since the latest game patch may cause errors for that reason alone, and may even cause cascading failures in other seemingly unrelated mods.
- *The issue is a duplicate.* Skim all open issues before posting a new one to make sure an identical or similar issue isn't already tracked. Duplicate issues will be closed.

### Describe the Bug
A clear and concise description of what the bug is.

### To Reproduce
Steps to reproduce the behavior:
1. Go to '...'
2. Click on '....'
3. Scroll down to '....'
4. See error

### Expected Behavior
A clear and concise description of what you expected to happen.

### Console Log
**Please include a link or embed of a FULL console log file from a run of the game where the bug happened**. This will provide a versioned mod list, and possibly other important information about the bug.

*To retrieve console logs:* A log from the most recent run of the game will be stored in `.../Risk of Rain 2/BepInEx/LogOutput.log`. If using r2modman, go to Settings > Browse profile folder > `Profile Folder/BepInEx/LogOutput.log` instead.

*If the bug only happens in multiplayer:* try to provide console logs from both client and server.

*If the log file is too big to post:* this may be because of a once-per-frame error message. In this case, immediately close the game once the bug occurs to reduce log file size.

### Additional Context
Add any other context about the problem here (screenshots, etc.).
