#/bin/bash
pushd ../js
uglifyjs clean-blog.js -o clean-blog.min.js --compress --mangle
uglifyjs jquery.js -o jquery.min.js --compress --mangle
popd

pushd ../css
minify --output clean-blog.min.css clean-blog.css
minify --output bootstrap.min.css bootstrap.css
popd
