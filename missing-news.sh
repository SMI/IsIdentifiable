#!/bin/bash

if [ ! -d news ]
then
  echo Must be run from main SmiServices directory!
  exit
fi

lasttag=$(git describe --tags --abbrev=0)

for i in `git log --oneline ${lasttag}..|egrep -v "dependabot|Bump|pre-commit-ci|snyk-fix" | grep -Po 'Merge pull request #\d+'|cut -d'#' -f2`
do
  if ! find news -name "${i}*.md" -print -quit | grep -q .
  then
    echo -n "Missing news file for PR #${i}:"
    curl -s https://api.github.com/repos/SMI/IsIdentifiable/pulls/${i} | grep \"title\"|cut -d'"' -f4 | grep . || echo
  fi
done
