## Podnanza: Podcast Feeds for Danish Radio (DR) Bonanza Archive

Podnanza is a screen-scraper and feed-generator that turns radio series from the [Danish Radio Bonanza archive](https://www.dr.dk/bonanza/) into podcast feeds for easy listening in your favorite podcast app. Check out the [blog post](https://www.friism.com/podnanza-podcast-feeds-for-danish-radio-dr-bonanza-archive/) for details on what the Podnanza software does and how to subscribe to the podcast feeds it generates.


## Deploying

The Podnanza software can run as a "normal" ASP.NET Core app or on AWS Lambda. To deploy on Lamdba, install:

* [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/install-windows.html)
* [SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/serverless-sam-cli-install.html)
* [AWS .NET Core CLI](https://docs.aws.amazon.com/lambda/latest/dg/lambda-dotnet-coreclr-deployment-package.html)

Once logged in and set up, run this command in the `Podnanza` project folder to deploy a new version:

```
dotnet lambda deploy-serverless --region eu-north-1 --s3-bucket podnanza --stack-name Podnanza .
```
