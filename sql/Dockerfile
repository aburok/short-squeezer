FROM mcr.microsoft.com/mssql/server:2022-latest

# Install necessary packages for SQL Server tools
USER root
RUN apt-get update && apt-get install -y \
    curl \
    gnupg \
    && curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
    && curl https://packages.microsoft.com/config/ubuntu/20.04/prod.list > /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y mssql-tools unixodbc-dev \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Add SQL Server tools to the PATH
ENV PATH="${PATH}:/opt/mssql-tools/bin"

# Switch back to mssql user
USER mssql