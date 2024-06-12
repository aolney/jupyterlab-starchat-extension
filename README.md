# aolney_jupyterlab_starchat_extension

[![Github Actions Status](https://github.com/aolney/jupyterlab-starchat-extension/workflows/Build/badge.svg)](https://github.com/aolney/jupyterlab-starchat-extension/actions/workflows/build.yml)
[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/aolney/jupyterlab-starchat-extension/main?urlpath=lab)

> **Note**
> You will need a StarChat endpoint. See [starchat-service](https://github.com/aolney/starchat-service)

A [JupyterLab](https://jupyterlab.readthedocs.io/en/stable/) extension that implements the [StarChat](https://huggingface.co/spaces/HuggingFaceH4/starchat-playground) coding assistant. U

The following query string parameters enable functionality:

- `endpoint=xxx` specifies a url for the StarChat endpoint (e.g. https://yourdomain.com/starchat) **the extension will not work properly without this**
- `log=xxx` specifies a url for a logging endpoint (e.g. https://yourdomain.com/log)
- `id=xxx` adds an identifier for logging

The log data format is `{username: string, json: string}`.

## Requirements

- JupyterLab >= 4.0.0

## Install

To install the extension, execute:

```bash
pip install aolney_jupyterlab_starchat_extension
```

## Uninstall

To remove the extension, execute:

```bash
pip uninstall aolney_jupyterlab_starchat_extension
```

## Contributing

### Development install

Note: You will need NodeJS to build the extension package.

The `jlpm` command is JupyterLab's pinned version of
[yarn](https://yarnpkg.com/) that is installed with JupyterLab. You may use
`yarn` or `npm` in lieu of `jlpm` below.

```bash
# Clone the repo to your local environment
# Change directory to the aolney_jupyterlab_starchat_extension directory
# Install package in development mode
pip install -e "."
# Link your development version of the extension with JupyterLab
jupyter labextension develop . --overwrite
# Rebuild extension Typescript source after making changes
jlpm build
```

You can watch the source directory and run JupyterLab at the same time in different terminals to watch for changes in the extension's source and automatically rebuild the extension.

```bash
# Watch the source directory in one terminal, automatically rebuilding when needed
jlpm watch
# Run JupyterLab in another terminal
jupyter lab
```

With the watch command running, every saved change will immediately be built locally and available in your running JupyterLab. Refresh JupyterLab to load the change in your browser (you may need to wait several seconds for the extension to be rebuilt).

By default, the `jlpm build` command generates the source maps for this extension to make it easier to debug using the browser dev tools. To also generate source maps for the JupyterLab core extensions, you can run the following command:

```bash
jupyter lab build --minimize=False
```

### Development uninstall

```bash
pip uninstall aolney_jupyterlab_starchat_extension
```

In development mode, you will also need to remove the symlink created by `jupyter labextension develop`
command. To find its location, you can run `jupyter labextension list` to figure out where the `labextensions`
folder is located. Then you can remove the symlink named `@aolney/jupyterlab-starchat-extension` within that folder.

### Testing the extension

#### Frontend tests

This extension is using [Jest](https://jestjs.io/) for JavaScript code testing.

To execute them, execute:

```sh
jlpm
jlpm test
```

#### Integration tests

This extension uses [Playwright](https://playwright.dev/docs/intro) for the integration tests (aka user level tests).
More precisely, the JupyterLab helper [Galata](https://github.com/jupyterlab/jupyterlab/tree/master/galata) is used to handle testing the extension in JupyterLab.

More information are provided within the [ui-tests](./ui-tests/README.md) README.

### Packaging the extension

See [RELEASE](RELEASE.md)
