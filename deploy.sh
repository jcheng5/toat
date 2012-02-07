#!/bin/sh

rsync -rPt --delete --exclude=.idea --exclude=.DS_Store website/site/ occsoft@berbils.dreamhost.com:/home/occsoft/teeoffagainsttrafficking.com/

