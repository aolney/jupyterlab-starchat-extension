# jupyterlab-starchat-extension

> **Note**
> You will need a StarChat endpoint. See [starchat-service](https://github.com/aolney/starchat-service)

A [JupyterLab](https://jupyterlab.readthedocs.io/en/stable/) extension that implements the [StarChat](https://huggingface.co/spaces/HuggingFaceH4/starchat-playground) coding assistant. Uses [Fable](https://fable.io/) tooling.

The following query string parameters enable functionality:

- `endpoint=xxx` specifies a url for the StarChat endpoint (e.g. https://yourdomain.com/starchat) **the extension will not work properly without this**
- `log=xxx` specifies a url for a logging endpoint (e.g. https://yourdomain.com/log)
- `id=xxx` adds an identifier for logging

The log data format is `{username: string, json: string}`.

**NOTE: This plugin requires jupyterlab <= 1.2.6, so if you have a higher version (e.g. 2.X) you will need to execute `conda install jupyterlab=1.2.6` or similar for `pip`. The conda environment specification provided in `environment.yml` will work as-is (e.g. `mamba env create -f environment.yml`) or can be used as a template.**

## Installation

```bash
jupyter labextension install @aolney/jupyterlab-starchat-extension
```

## Updating to latest version

```bash
jupyter labextension update @aolney/jupyterlab-starchat-extension
```


## Development

This is based on my personal preferences. For more options, [see the extension development guide](https://jupyterlab.readthedocs.io/en/stable/developer/extension_dev.html#developer-extensions).

### Prerequisites

* [JupyterLab](https://jupyterlab.readthedocs.io/en/stable/getting_started/installation.html)
* [Fable](https://fable.io/)
* An F# editor like Visual Studio Code with [Ionide](http://ionide.io/) 
* Chrome
* 
### Initial install and after library adds

```bash
npm install
mono .paket/paket.exe install
npm run build
```

### Terminal A in VSCode

```bash
jupyter labextension install . --no-build
npm run watch
```

This will watch your F# code and trigger builds of `index.js`.

If you prefer not to trigger builds using a watch, you can `npm run build` every time you want a new build.

### Terminal B in VSCode

```bash
jupyter lab --watch
```

This will watch your extension and trigger builds of it.

Even with this watch, you still need to refresh your browser during development.

## Project structure

### npm/yarn

JS dependencies are declared in `package.json`, while `package-lock.json` is a lock file automatically generated.

### paket

[Paket](https://fsprojects.github.io/Paket/) 

> Paket is a dependency manager for .NET and mono projects, which is designed to work well with NuGet packages and also enables referencing files directly from Git repositories or any HTTP resource. It enables precise and predictable control over what packages the projects within your application reference.

.NET dependencies are declared in `paket.dependencies`. The `src/paket.references` lists the libraries actually used in the project. Since you can have several F# projects, we could have different custom `.paket` files for each project.

Last but not least, in the `.fsproj` file you can find a new node: `	<Import Project="..\.paket\Paket.Restore.targets" />` which just tells the compiler to look for the referenced libraries information from the `.paket/Paket.Restore.targets` file.

### Fable-splitter

[Fable-splitter]() is a standalone tool which outputs separated files instead of a single bundle. Here all the js files are put into the `lib`. And the main entry point is our `index.js` file.

### Imports

Because Jupyter uses Typescript, we can use [ts2fable](https://github.com/fable-compiler/ts2fable) to generate strongly typed imports of Jupyter's JS packages. Unfortunately these are a bit huge and the conversion is messy. `Jupyter.fs` contains the minimal imports for this demo repo, and these imports were tweaked out of the files in the `ts2fable-attempt` directory.


