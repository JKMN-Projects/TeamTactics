ALTER TABLE team_tactics.user_account 
        ADD CONSTRAINT user_account_email_unique UNIQUE (email),
        ADD CONSTRAINT user_account_username_unique UNIQUE (username);