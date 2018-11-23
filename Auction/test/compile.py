#!/usr/bin/python

from subprocess import call, DEVNULL

def compile_contracts(tests):
      # Compile main PASS.cs contract to .exe and .dll
      # TODO: compile using `dotnet publish` instead of csc
      call(["csc", "../../PASS/PASS.cs", "/reference:Pravda.dll", "/debug:portable", "/target:library"], stdout=DEVNULL)
      call(["csc", "../../PASS/PASS.cs", "/reference:Pravda.dll", "/debug:portable"], stdout=DEVNULL)
      call(["csc", "../Auction.cs", "/reference:Pravda.dll", "/reference:PASS.dll", "/debug:portable"], stdout=DEVNULL)
      call(["csc", "../Auction.cs", "/reference:Pravda.dll", "/reference:PASS.dll", "/debug:portable", "/target:library"], stdout=DEVNULL)

      # Generate Pravda-code
      call(["pravda", "compile", "dotnet", "--input",
            "Auction.exe,Auction.pdb,PASS.exe,PASS.pdb", 
            "--output", "Auction.pravda", "--main-class", "auction.Auction"])

      print("Auction.pravda compiled")

      call(["pravda", "compile", "dotnet", "--input", "PASS.exe,PASS.pdb", 
            "--output", "PASS.pravda", "--main-class", "auction.PASS"])

      print("PASS.pravda compiled")

      for test in tests:
            # Compile to .exe
            call(["csc", "pcalls/{}.cs".format(test), "/reference:Pravda.dll", "/reference:Auction.dll", "/reference:PASS.dll",
                  "/debug:portable", "/out:pcalls/{}.exe".format(test)], stdout=DEVNULL)

            # Generate Pravda-code
            call(["pravda", "compile", "dotnet", "--input",
                  "pcalls/{}.exe,pcalls/{}.pdb,PASS.exe,PASS.pdb,Auction.exe,Auction.pdb".format(test, test), "--output",
                  "pcalls/{}.pravda".format(test), "--main-class", "auction.{}".format(test)])

            # A litle clean-up
            call(["rm", "-rf", "pcalls/{}.exe".format(test), "pcalls/{}.pdb".format(test)])

            print("{}.pravda compiled".format(test))

      # Some more clean-up
      call(["rm", "-rf", "Auction.exe", "Auction.dll", "Auction.pdb"])
      call(["rm", "-rf", "PASS.exe", "PASS.dll", "PASS.pdb"])

if __name__ == '__main__':
      compile_contracts([])