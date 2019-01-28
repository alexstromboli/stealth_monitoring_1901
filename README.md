# Stealth Monitoring Programming Exercise

This .NET Core application downloads Mars rover photos through NASA API for a specific date or a number of dates.

The application is buildable and runnable both in Docker and in pre-installed .NET Core environment.

## Command Line Arguments

### Mandatory

`<api-key>` NASA API key. It can be obtained on [NASA API website](https://api.nasa.gov/index.html).

### Optional

`<dates-file-path>` (As second argument) Specify text file where each line gives a date for which photos must be downloaded. The allowed date formats are listed below. Lines with misfit text are just ignored.

`--date <date>`, `-d <date>` Specify single date for which photos must be downloaded.
Date can be in one of the following formats:
* MM/dd/yy
* MMMM d, yyyy
* MMM-d-yyyy
The allowed formats are specified in `ReadDateTime.cs`

**Command line cannot have both `<dates-file-path>` and `--date`/`-d`**

If `<dates-file-path>` and `--date`/`-d` are both missing, the current date is taken.

`--index <photo-index>`, `-i <photo-index>` Specify a single photo that must be downloaded as a 1-based number among all of the photos of the day.

**`--index`/`-i` is only allowed for specific date, `--date`/`-d`**

`--open`, `-o` Automatically open the photo in a browser.
**Does not work in Docker.**
**Requires `--index`/`-i`**

`--outDir <dir-path>` Specify output directory.
If `--outDir` is missing, the current directory is taken.

### Examples

`sh run-docker.sh ENmMPqdDkumwcsb`
Download all photos of today. (Usually there are none, as photos only become available with a few hours delay.)

`sh run-docker.sh ENmMPqdDkumwcsb --date Jan-2-2019`
Download all photos of January 2, 2019.

`sh run-docker.sh ENmMPqdDkumwcsb -d Nov-3-2018 -i 1`
Download the first photo of November 3, 2018.

`sh run-docker.sh ENmMPqdDkumwcsb dates.txt`
Download all the photos for dates specified in `dates.txt`.

`dotnet run ENmMPqdDkumwcsb -d Nov-3-2018 -i 1 --open`
Download and display in browser the first photo of November 3, 2018.

## Output

For each date the application creates a directory named as `yyyy-MM-dd` and stores each photo as JPEG file named as `{photo-id}-{rover-name}-{camera-code}.jpg`.

## Use in Docker

### Prerequisites

The machine must have Docker engine pre-installed. The command line interface of the engine must be accessible through `docker` command. No pre-installed images are required.

Where appropriate, the scripts must be run as `sudo`.

### Build

To build the application run
```
sh build-docker.sh
```

Don't forget to use `sudo` if your account privileges are not elevated to run `docker`.
First run can take a few minutes, as necessary images will be pulled (.NET Core build tools and runtime).

### Run

To run the application run
```
sh run-docker.sh <arguments, see above>
```

Again, don't forget to use `sudo` if necessary.

### Docker for Windows Considerations

Input file (with dates) and output directory must be within directories that are "shared" with the Docker engine virtual machine. Otherwise they won't be accessible by the application.

## Use in .NET Core

### Prerequisites

The machine must have .NET Core with build tools and runtime pre-installed.

### Build

To build the application run
```
cd GetchMarsRoverPhoto
dotnet build
```

### Run

To run the application run
```
cd GetchMarsRoverPhoto
dotnet run <arguments, see above>
```

## Errors

In case of faults and errors the application prints descriptive information and terminates. For demonstration purposes, where available, it also prints exception information.
