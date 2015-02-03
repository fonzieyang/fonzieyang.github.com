#/bin/bash
t=`date "+%Y-%m-%d%n"`
echo $t
mv $1 ../_posts/$t-$1
