# File utilities

[![Build status](https://ci.appveyor.com/api/projects/status/xn4k826bcvg3niaj?svg=true)](https://ci.appveyor.com/project/arsuq/files)

Put files in a PATH folder and launch from anywhere.
Some utils support changing the source dir, but the default is the current location.

Launch a specific subprogram with the -p switch:

```
files -p take
```
or
```
dotnet <path-to-filles>/files.dll -p take
```


All subprograms start in interactive mode, unless the -ni switch is present and supported.

```
files -p ext -ni -sp *.png -ext jpg
```

### colpick 
   Picks columns by index from a file and copies them into a new file.
   Can also be used to rearrange the columns in a csv file.
   Args:

    not interactive (-ni)
    file (-in)
    output file (-out)
    separator (-sep)
    columns (-cols) ex: -cols 1, 4

### concat
   Concatenates text files.
   Args: 

    not interactive (-ni)
    out-file (-out)
    paths map file  [separated by new line] (-files)
    add a new line before each file content (-fsep)

### fcopy
   Copies or moves files listed in a text file. Each file path must be on a separate line.
   Args:

    not interactive (-ni)
    map file (-f)
    destination dir (-dst)
    move [default is copy] (-m)
    quiet (do not report missing) (-q)

### fdelete
   Deletes files listed in a text file. Each file path must be on a separate line.
   Args:

    not interactive (-ni)
    map file (-tf)
    quiet (do not report missing) (-q)

### duplicates
   Detects file duplicates in one or more folders by comparing sizes, names or data hashes.
   There are extension and size filters as well as option for partial hashing
   by skip/taking portions of the files.

### ext
   Recursively changes the extensions of all matched files.  
   Args:

	not interactive (-ni)
	source dir [default is current] (-src)
	search pattern [*.*] (-sp), extension (-ext)

### foreach
   Enumerates all matching files in a given root and launches a process with the
   provided args.
   Args: 
  
    not interactive (-ni)
    concurrent instances [1] (-ci)
    root [current] (-root)
    file search (-sp) [*.*]
    recursive (-rec) [y/n]
    program to call [files] (-proc)
    program args (-pargs) [put in quotes and add a single space before the pargs string] 
    current dir full path as arg name (-cdf)
    current dir as arg (-cd)
     Example: files -p foreach -ni -cdf $$ -cd $ -root <path> -proc <files>
    -pargs " -p colpick -ni  -in $$ -out $$ -cols 0,4,1 "

### get 
   Downloads resources listed in a map file with each link on a separate line.  
   Args: 

	not interactive (-ni)
	single url (-url)
	links file (-f)
	base url (-base)
	destination dir (-dest)
	max req/sec [5.0] (-rps)
	from file row [0] (-from)
	to file row [last] (-to)

### insert
   Inserts/overrides the beginning or the end of all matching files with custom content.  
   Args:
	
	not interactive (-ni)
	source dir [default is current] (-src)
	search pattern [*.*] (-sp)
	ignore regex [optional Ex: (?:path\A|path\B)] (-ireg)
	recursive [y/*] (-r)
	text file [if not -txt] (-tf)
	text [if not -tf] (-txt)
	append [by default prepends] (-append)
	override [by default inserts] (-ovr)

### lclones
   Removes line clones in a text file.If -paths parses each line as a path and removes duplicate filenames.
   Args: 
    
    not interactive (-ni)
    source text file (-src)
    output text file (-dst)
    clones file (-clones)
    lines are paths (-paths)

### linefix
   Includes text at the beginning or at the end of each line in a text file
   Args:

    not interactive (-ni)
    text file (-tf)
    text (-text)
    output file (-out)
    append [by default prepends] (-append)

### logrestore
   Creates a file with paths (log), which can be used to move files (restore).

### lhaving
   Takes matching lines from a text file and saves them to another.
   Args:

    not interactive (-ni)
    text file (-tf)
    search text (-text) 
    output file (-out)
    except [takes all but the matched lines] (-x)

### lrand
   Randomizes the lines in a text file. Note that lrand will override the original file!
   Args: 
 
   not interactive (-ni), text file (-tf)

### lsort
   Sorts the lines in a text file in ascending or descending order. This will
   override the original file!
   Args: 

    not interactive (-ni)
    text file (-tf) 
    descending order [by default is asc] (-desc)

### move
   Moves the matching files to DestinationDir/Prefix + Counter. Can be used as
   rename.
   Args:

    not interactive (-ni)
    source (-src)
    destination dir (-dest)
    prefix (-prf)
    indexing counter (-ic)
    ic step (-step)
    index length with zero padding (-zpad)
    sort options (-sort) 
         0 - no 
         1 - asc name
         2 - desc name 
         3 - randomize 
         4 - asc created date time (default)
         5 - desc created date time

### pad
   Adds leading zeros to the numbers in the file names.

### rename
   Renames the matching files from the current folder with the names from a text
   file.

### replace
   Replaces the matching text with a given string in all/some files in a folder.
   Args: 

    interactive (-ni)
    source dir [default is current] (-src)
    search pattern [*.*] (-sp)
    target regex (-reg)
    text to replace (-txt)
    ignore path regex [optional, Ex: (?:path\A|path\B)] (-ireg)
    recursive [y/*] (-r)

   Example converting CRLF line endings to LF:
    
     files -p replace -ni -src <dir> -reg (?:\r\n) -text \n

   Example normal text replacement: 
 
    files -p replace -ni -src <dir> -reg "text with spaces" -text "new text"

### search
   Search for files recursively.
   Args:

    not interactive (-ni)
    source (-src)
    search pattern (-sp)

### take
   Copies or moves n random files from the current folder into a destination folder.    
   Args:
	
	not interactive (-ni)
	source dir [default is current] (-src)
	destination dir (-dest)
	recursive [y/*] (-r)
	mirror dir tree [if recursive] (-mt)
	search pattern [*.*] (-sp)
	full-path regex [Ex: (?:dirA|dirB)] (-reg)
	ignore files smaller than [in KB] (-ist)
	ignore files bigger than [in KB] (-ibt)
	prefix the taken files with (-prf)
	take count (-take)
	move [default is copy mode] (-move)

### traverse 
   Traverses all sub-folders of a given root and launches a process with the provided args.
   If specified replaces a template string in the args with the current folder full path or name.   
   Args: 
 
    root (-root)
    program to call (-proc)
    program args (-pargs) Put in quotes and add a single space before the pargs string.
    current dir full path as arg name (-cdf)
    current dir as arg (-cd)
    Example:
	   Renames all files by prefixing them with their folder name.
       files -p traverse -ni -cdf $$ -cd $ -root <path> -proc <files>
       -pargs " -p move -ni -zpad 3 -src $$ -dest $$ -prf $-"
       Note the space before the -p move!
