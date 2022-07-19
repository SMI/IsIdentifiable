#!/bin/sh
sudo apt install -y musl-tools build-essential
export TOOLCHAIN=$(pwd)/x86_64-linux-musl-native
if ! [ -d $TOOLCHAIN ]
then
	curl -sL https://more.musl.cc/10/x86_64-linux-musl/x86_64-linux-musl-native.tgz | tar xzf -
fi
export CC=$TOOLCHAIN/bin/gcc
export PATH=$TOOLCHAIN/bin:$PATH
if ! [ -d zlib-1.2.11 ]
then
	curl -sL https://zlib.net/zlib-1.2.11.tar.xz | tar xJf -
	cd zlib-1.2.11
	./configure --prefix=$TOOLCHAIN --static
	make -sj4
	make install
	cd -
fi
mvn -B -q clean compile assembly:single
java -agentlib:native-image-agent=config-output-dir=META-INF/native-image -jar target/nerd-0.0.1-SNAPSHOT.jar &
timeout 30 sh -c 'until nc -z 127.0.0.1 1881; do sleep 1; done'
printf "University of Dundee\0Fred Bloggs\0Ninewells Hospital\0person\0woman\0man\0camera\0tv\0" | nc -N 127.0.0.1 1881
kill $!
/usr/lib/jvm/graalvm/bin/native-image -H:+ReportExceptionStackTraces --no-fallback --initialize-at-build-time=uk.ac.dundee.hic.nerd.Program,edu.stanford.nlp.ling,edu.stanford.nlp.sequences,edu.stanford.nlp.util,edu.stanford.nlp.ie,org.slf4j.LoggerFactory -H:IncludeResources=".*ser\\.gz$" -jar target/nerd-0.0.1-SNAPSHOT.jar

