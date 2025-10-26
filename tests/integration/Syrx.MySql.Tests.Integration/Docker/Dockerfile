# Syrx MySQL Test Database Docker Image
# This image provides a pre-configured MySQL instance with all test objects for Syrx.MySql.Tests.Integration

FROM mysql:8.0

# Set environment variables for MySQL
ENV MYSQL_DATABASE=syrx
ENV MYSQL_USER=syrx_user
ENV MYSQL_PASSWORD=YourStrong!Passw0rd
ENV MYSQL_ROOT_PASSWORD=YourStrong!Passw0rd

# Copy initialization scripts
COPY init-scripts/*.sql /docker-entrypoint-initdb.d/

# Expose MySQL port
EXPOSE 3306

# The base mysql image already handles initialization scripts in /docker-entrypoint-initdb.d
# so we don't need a custom entrypoint