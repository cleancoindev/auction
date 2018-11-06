#!/usr/bin/python

from subprocess import call

def compile_contracts():
      # Compile main PASS.cs contract
      call(["csc", "../PASS.cs", "/reference:Pravda.dll", "/debug:portable"])
      call(["csc", "../PASS.cs", "/reference:Pravda.dll", "/debug:portable", "/target:library"])
      call(["pravda", "compile", "dotnet", "--input",
            "PASS.exe,PASS.pdb", "--output", "PASS.pravda"])

      # Compile testing contracts
      tests = ['emit']
      for test in tests:
            call(["csc", "pcalls/{}.cs".format(test), "/reference:Pravda.dll", "/reference:PASS.exe",
                  "/debug:portable", "/out:pcalls/{}.exe".format(test)])
            call(["pravda", "compile", "dotnet", "--input",
                  "pcalls/{}.exe,pcalls/{}.pdb,PASS.dll,PASS.pdb".format(test, test), "--output",
                  "pcalls/{}.pravda".format(test), "--main-class", "PcallNamespace.PASS"])
            call(["rm", "-rf", "pcalls/{}.exe".format(test), "pcalls/{}.pdb".format(test)])

      call(["rm", "-rf", "PASS.exe", "PASS.dll", "PASS.pdb"])

if __name__ == '__main__':
      compile_contracts()