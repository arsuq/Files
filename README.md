# File utilities

Put the dlls in a PATH folder and launch from anywhere.
Some utils support changing the source dir, but the default is the current location.

Launch a specific subprogram with the -p switch:

```
 files -p take <args>
```

All subprograms start in interactive mode, unless the -ni switch is present and supported.

```
files -p ext -ni -sp *.png -ext jpg
```

### duplicates
   Detects file duplicates in one or more folders by comparing sizes, names or
   data hashes.
   There are extension and size filters as well as option for partial hashing
   by skip/taking portions of the files.

### ext
   Recursively changes the extensions of all matched files.
   Args: not interactive (-ni), source dir [default is current] (-src), search
   pattern [*.*] (-sp), extension (-ext)

### logrestore
   Creates a file with paths (log), which can be used to move files (restore).

### move
   Moves the matching files from the current dir as DestinationDir/Prefix +
   Counter. Can be used as rename.

### pad
   Adds leading zeros to the numbers in the file names.

### rename
   Renames the matching files from the current folder with the names from a text
   file.

### take
   Copies or moves n random files from the current folder into a destination
   folder.
   Args: not interactive (-ni), source dir [default is current] (-src),
   destination dir (-dest), search pattern [*.*] (-sp), prefix (-prf), take
   count (-take) move [default is copy mode] (-move)