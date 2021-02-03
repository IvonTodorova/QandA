/** @jsxRuntime classic */
/** @jsx jsx */
import { css, jsx } from '@emotion/react';
//import { fontFamily, fontSize, gray1, gray2, gray5 } from './Styles';
import user from './user.svg';

export const UserIcon = () => (
  <img
    src={user}
    alt="User"
    css={css`
      width: 12px;
      opacity: 0.6;
    `}
  />
);
