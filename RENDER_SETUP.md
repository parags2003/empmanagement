# Complete Render Setup Guide for Employee Leave Management

## Step-by-Step Instructions

### 1. **Create/Verify PostgreSQL Database on Render**

1. Go to [Render Dashboard](https://dashboard.render.com/)
2. Click **"New +"** → **"PostgreSQL"**
3. Choose **"Free"** tier
4. Give it a name (e.g., `empmanagement-db`)
5. Click **"Create Database"**
6. Wait for it to be ready (takes 1-2 minutes)
7. Once ready, go to the database dashboard
8. Copy the **"Internal Database URL"** or **"External Connection String"**
   - It will look like: `postgresql://user:password@host:port/database`
   - **Save this - you'll need it in step 3**

### 2. **Create Web Service on Render**

1. In Render Dashboard, click **"New +"** → **"Web Service"**
2. Connect your GitHub repository: `parags2003/empmanagement`
3. Configure the service:
   - **Name**: `empmanagement` (or any name you prefer)
   - **Region**: Choose closest to you
   - **Branch**: `master` (or your main branch)
   - **Runtime**: `.NET`
   - **Build Command**: `dotnet restore && dotnet publish -c Release -o out`
   - **Start Command**: `dotnet out/EmployeeLeaveManagement.dll`
4. Click **"Create Web Service"**

### 3. **Configure Environment Variables** ⚠️ **CRITICAL STEP**

1. In your Web Service dashboard, go to **"Environment"** tab
2. Click **"Add Environment Variable"**
3. Add these variables:

   **Variable 1:**
   - **Key**: `ASPNETCORE_ENVIRONMENT`
   - **Value**: `Production`
   - Click **"Save Changes"**

   **Variable 2:**
   - **Key**: `ConnectionStrings__DefaultConnection`
   - **Value**: Paste your PostgreSQL connection string from Step 1
     - You can use either format:
       - URL format: `postgresql://user:pass@host:port/db` ✅ (Recommended)
       - Standard format: `Host=host;Port=5432;Database=db;Username=user;Password=pass;SSL Mode=Require;Trust Server Certificate=true`
   - Click **"Save Changes"**

4. **Important**: Make sure both variables are saved!

### 4. **Run Database Migrations** 🔧

After the first deployment, you need to run migrations to create tables:

**Option A: Using Render Shell (Recommended)**
1. In your Web Service dashboard, click **"Shell"** tab
2. Wait for the shell to connect
3. Run these commands:
   ```bash
   cd /opt/render/project/src
   dotnet ef database update
   ```
4. Wait for migrations to complete
5. You should see: `Done.` when successful

**Option B: Using Build Script (Automatic)**
- See the `render-build.sh` script in the repository
- This will run migrations automatically on each deploy

### 5. **Verify Connection**

1. Go to your Web Service **"Logs"** tab
2. Look for this message:
   ```
   ✅ Successfully connected to PostgreSQL database.
   ```
3. If you see this, the connection is working! ✅
4. If you see errors, check the troubleshooting section below

### 6. **Test Your Application**

1. Visit your Render URL: `https://your-service-name.onrender.com`
2. Navigate to: `https://your-service-name.onrender.com/Employee`
3. Try to:
   - Create a new employee
   - View employee list
   - Apply for leave
   - Check leave management

## Troubleshooting

### ❌ Connection String Not Found Error
**Problem**: `Connection string 'DefaultConnection' not found`
**Solution**: 
- Verify `ConnectionStrings__DefaultConnection` is set in Environment variables
- Make sure you used **two underscores** (`__`) not one (`_`)
- Restart the service after adding environment variables

### ❌ Database Connection Failed
**Problem**: `Failed to connect to PostgreSQL database`
**Solution**:
1. Check if PostgreSQL service is running (green status)
2. Verify connection string is correct
3. Make sure you're using the **Internal Database URL** (not external)
4. Check if migrations have been run (see Step 4)

### ❌ Tables Don't Exist / 500 Errors
**Problem**: CRUD operations fail with database errors
**Solution**:
- Run migrations: `dotnet ef database update` in Render Shell
- Check logs for specific error messages
- Verify database is accessible from web service

### ❌ SSL Connection Errors
**Problem**: SSL-related connection errors
**Solution**:
- The code automatically adds SSL parameters, but if issues persist:
- Use the standard format connection string with explicit SSL settings
- Make sure you're using the Internal Database URL

## Quick Checklist

- [ ] PostgreSQL database created on Render
- [ ] Web Service created and connected to GitHub
- [ ] Environment variable `ASPNETCORE_ENVIRONMENT=Production` set
- [ ] Environment variable `ConnectionStrings__DefaultConnection` set with your database URL
- [ ] Service deployed successfully
- [ ] Migrations run (`dotnet ef database update`)
- [ ] Logs show: `✅ Successfully connected to PostgreSQL database`
- [ ] Can access the application URL
- [ ] CRUD operations work (Create, Read, Update, Delete)

## Important Notes

1. **Free Tier Limitations**:
   - Services spin down after 15 minutes of inactivity
   - First request after spin-down takes 30-60 seconds
   - Database has limited connections on free tier

2. **Connection String Format**:
   - The application automatically handles both URL and standard formats
   - SSL is automatically configured for cloud databases
   - No manual SSL configuration needed

3. **Migrations**:
   - Must be run at least once after first deployment
   - If you add new migrations, run `dotnet ef database update` again
   - Consider using the build script for automatic migrations

4. **Updates**:
   - Every push to your GitHub repository triggers automatic deployment
   - Environment variables persist across deployments
   - Database data persists (unless you delete the database)

## Need Help?

If you're still having issues:
1. Check the **Logs** tab in Render for error messages
2. Verify all environment variables are set correctly
3. Make sure migrations have been run
4. Test the connection string locally if possible

