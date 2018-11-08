organization := "com.example"

name := "auction"

version := "0.1.0-SNAPSHOT"

scalaVersion := "2.12.7"
ensimeScalaVersion := "2.14.7"

val korolevVersion = "0.9.0"

resolvers += "Expload Bintray" at "https://dl.bintray.com/expload/oss/"

libraryDependencies ++= Seq(
  "org.slf4j" % "slf4j-simple" % "1.7.+",
  "com.github.fomkin" %% "korolev-server-blaze" % korolevVersion,
  "com.tethys-json" %% "tethys" % "0.7.0.2",
  "com.typesafe.akka" %% "akka-actor" % "2.5.8",
  "com.typesafe.akka" %% "akka-stream" % "2.5.8",
  "com.typesafe.akka" %% "akka-http" % "10.0.11",
  "com.github.fomkin" %% "korolev-server-akkahttp" % "0.6.1",
  "com.expload" %% "pravda-node" % "0.11.0"
)
