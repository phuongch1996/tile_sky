# Leaderboards Authoring

This module allows users to author, modify, and deploy Leaderboards assets directly from the Unity Editor.

> NOTE1: Leaderboards Authoring is only supported on Unity 2021.3 and above.
>
> NOTE2: Leaderboards Authoring is enabled only if the [Deployment package](https://docs.unity3d.com/Packages/com.unity.services.deployment@latest) is installed.

## Deployment Window

The Deployment Window is a core feature of the Deployment package.

The purpose of the Deployment Window is to allow all services
to have a single cohesive interface for Deployment needs.

The Deployment Window provides a uniform deployment interface for all services.
It allows you to upload cloud assets for your respective cloud service.

For more information, consult the [com.unity.services.deployment](https://docs.unity3d.com/Packages/com.unity.services.deployment@latest) package documentation.

## Create Leaderboards Assets

Right-click on the `Project Window` then select `Create > Services > Leaderboard Configuration` to create a Leaderboard Configuration file.

The Deployment Window automatically detects these files to be deployed at a later time.

For more information on how to create and modify Currency Assets,
please see the [Leaderboard Configuration assets](./leaderboard_configuration.md) documentation.
