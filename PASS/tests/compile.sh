csc ../PASS.cs /reference:Pravda.dll /debug:portable &&
pravda compile dotnet --input PASS.exe,PASS.pdb --output PASS.pravda &&
echo "Compilation successful!"