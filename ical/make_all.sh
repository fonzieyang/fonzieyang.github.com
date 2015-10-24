#!/bin/bash

osascript exportTyme.scpt
mv ~/Downloads/tyme_for_cal.csv ./tyme_for_cal.csv
python ConvCsvToiCal.py

git add .
git commit -m "update cal"
git push
