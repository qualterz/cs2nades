# CS2Nades

CLI utility to list nades from Counter-Strike 2 demos.

Usage: `cs2nades <path to .dem file>`

It outputs JSON lines, so it's good to combine it with `jq`, `grep` utilities, for example, `cs2nades demo.dem | grep Blue | jq .console`.