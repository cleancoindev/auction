#!/usr/bin/python

from subprocess import call, DEVNULL

def compile_contracts(tests):
      # Compile main PASS.cs contract to .exe and .dll
      # TODO: compile using `dotnet publish` instead of csc
      call(["csc", "../PASS.cs", "/reference:Pravda.dll", "/debug:portable"], stdout=DEVNULL)
      call(["csc", "../PASS.cs", "/reference:Pravda.dll", "/debug:portable", "/target:library"], stdout=DEVNULL)

      # Generate Pravda-code
      call(["pravda", "compile", "dotnet", "--input",
            "PASS.exe,PASS.pdb", "--output", "PASS.pravda"])

      print("PASS.pravda compiled")

      for test in tests:
            # Compile to .exe
            call(["csc", "pcalls/{}.cs".format(test), "/reference:Pravda.dll", "/reference:PASS.dll",
                  "/debug:portable", "/out:pcalls/{}.exe".format(test)], stdout=DEVNULL)

            # Generate Pravda-code
            call(["pravda", "compile", "dotnet", "--input",
                  "pcalls/{}.exe,pcalls/{}.pdb,PASS.exe,PASS.pdb".format(test, test), "--output",
                  "pcalls/{}.pravda".format(test), "--main-class", "PcallNamespace.{}".format(test)])

            # A litle clean-up
            call(["rm", "-rf", "pcalls/{}.exe".format(test), "pcalls/{}.pdb".format(test)])

            print("{}.pravda compiled".format(test))

      # Some more clean-up
      call(["rm", "-rf", "PASS.exe", "PASS.dll", "PASS.pdb"])

if __name__ == '__main__':
      compile_contracts(['emit', 'transfer'])