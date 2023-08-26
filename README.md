# Localise-OpenAI-Resx-Toolkit
This small console .NET app will translate your resx files via OpenAI GPT models.

# License
THIS IS PROVIDED AS IT IS. I TAKE ZERO RESPONSIBILITY OF THE PROVIDED CODE/APP. 
If you want feature/fix - I won't do that for free - sorry, you can make fix it by yourself though and push pull request.
I just opensource what I did to speed-up my internal developer flow.

# How to use?
Clone, build and debug or build release and open in terminal like here:

![Screenshot 2023-08-26 at 19 13 48](https://github.com/thefex/Localise-OpenAI-Resx-Toolkit/assets/6318401/ddd5ac05-6aa6-43d1-b6b9-cde6fbc77c1f)

# How it works?
You provide base translations and folder with translations.
You need to manually create MyResources.culture-info.resx files.

It pushes resx key values, translate using OpenAI and save updated resx.

# Important notes
Check your translations after job done!
Sometimes you need to redo the task as it's not perfect. However... it works, sometimes you just need to run it twice, or clean "generated" translations and rerun - so it consume tokens twice

It speedups translations a lot though

# Showcase
![Screenshot 2023-08-26 at 20 45 30](https://github.com/thefex/Localise-OpenAI-Resx-Toolkit/assets/6318401/e541fb20-1723-4dce-a4f9-2003c6de64bd)
![Screenshot 2023-08-26 at 21 50 13](https://github.com/thefex/Localise-OpenAI-Resx-Toolkit/assets/6318401/09b41b3b-3358-434a-9e82-6b7d93899666)
![Screenshot 2023-08-26 at 21 47 54](https://github.com/thefex/Localise-OpenAI-Resx-Toolkit/assets/6318401/14421434-670a-4a8e-bd96-726c5df5b39c)
