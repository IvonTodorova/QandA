import { FC } from 'react';
/** @jsxRuntime classic */
/** @jsx jsx */
import { css, jsx } from '@emotion/react';
import { fontFamily, fontSize, gray5, gray2, gray6 } from './Styles';

interface Props {
  name: string;
  label?: string;
  type?: 'Text' | 'TextArea' | 'Password';
}

const baseCSS = css`
  box-sizing: border-box;
  font-family: ${fontFamily};
  font-size: ${fontSize};
  margin-bottom: 5px;
  padding: 8px 10px;
  border: 1px solid ${gray5};
  border-radius: 3px;
  color: ${gray2};
  background-color: white;
  width: 100%;
  :focus {
    outline-color: ${gray5};
  }
  :disabled {
    background-color: ${gray6};
  }
`;

export const Field: FC<Props> = ({ name, label, type = 'Text' }) => (
  <div
    css={css`
      display: flex;
      flex-direction: column;
      margin-bottom: 15px;
    `}
  >
    {label && (
      <label
        css={css`
          font-weight: bold;
        `}
        htmlFor={name}
      >
        {label}
      </label>
    )}
    {(type === 'Text' || type === 'Password') && (
      <input type={type.toLowerCase()} id={name} css={baseCSS} />
    )}
    {type === 'TextArea' && (
      <textarea
        id={name}
        css={css`
          ${baseCSS};
          height: 100px;
        `}
      />
    )}
  </div>
);
